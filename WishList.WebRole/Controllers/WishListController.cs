using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using WishList.WebRole.DataProviders;
using WishList.WebRole.Models;

namespace WishList.WebRole.Controllers
{
    public class WishListController : ApiController
    {
        private WishlistEntities db;

        private AzureBlobStorageProvider blobProvider;

        private const string blobContainer = "images";

        private static Dictionary<string, string> imageCache = new Dictionary<string, string>();

        public WishListController()
        {
            db = new WishlistEntities();
            db.Database.CommandTimeout = 60 * 1;
            blobProvider = new AzureBlobStorageProvider(ConfigurationManager.AppSettings["AzureBlobStorage"]); ;
        }

        [HttpGet]
        public async Task<List<WishItemContract>> Get(int? id = null)
        {
            IQueryable<WishItemContract> items = (from item in db.WishItem
                                                  select new WishItemContract
                                                  {
                                                      ID = item.ID,
                                                      name = item.name,
                                                      type = item.type,
                                                      brand = item.brand,
                                                      no = item.no,
                                                      comment = item.comment,
                                                      status = item.status,
                                                      feedback = item.feedback,
                                                      price = item.price,
                                                      currency = item.currency,
                                                      imageId = item.imageId,
                                                  });

            if (id.HasValue)
            {
                items = items.Where(x => x.ID == id.Value);
            }

            List<WishItemContract> selectedItems = items.ToList();

            foreach (WishItemContract item in selectedItems)
            {
                if (item.imageId == null)
                {
                    continue;
                }

                if (WishListController.imageCache.ContainsKey(item.imageId))
                {
                    item.base64 = WishListController.imageCache[item.imageId];
                }
                else
                {
                    if (await blobProvider.ExistsAsync(WishListController.blobContainer, item.imageId))
                    {
                        Stream stream = await blobProvider.GetBlobDataAsync(WishListController.blobContainer, item.imageId);
                        using (MemoryStream ms = new MemoryStream())
                        {
                            stream.Position = 0;
                            stream.CopyTo(ms);
                            item.base64 = Convert.ToBase64String(ms.ToArray());
                        }

                        WishListController.imageCache.Add(item.imageId, item.base64);
                    }
                }
            }

            return selectedItems;
        }

        [HttpPost]
        public async Task<HttpResponseMessage> Add()
        {
            // Check if the request contains multipart/form-data.
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            string root = HttpContext.Current.Server.MapPath("~/App_Data");
            if (!Directory.Exists(root))
            {
                Directory.CreateDirectory(root);
            }

            var provider = new MultipartFormDataStreamProvider(root);

            try
            {
                // Read the form data
                await Request.Content.ReadAsMultipartAsync(provider);

                // Get uploaded image
                Guid? guid = null;
                if (provider.FileData.Count > 0)
                {
                    guid = Guid.NewGuid();
                    MultipartFileData file = provider.FileData[0];
                    FileStream fs = new FileStream(file.LocalFileName, FileMode.Open);
                    BinaryReader br = new BinaryReader(fs);
                    byte[] bytes = br.ReadBytes((Int32)fs.Length);

                    // Get Image from bytes
                    Image image = null;
                    using (MemoryStream stream = new MemoryStream(bytes))
                    {
                        image = Image.FromStream(stream);
                    }

                    // Resize the image and compress
                    int width, height;
                    this.GetImageSize(image, 500, out width, out height);
                    Bitmap resized = this.ResizeImage(image, width, height);
                    Stream compressed = this.VaryQualityLevel(resized, 80L);

                    // Save blob to azure blob storage
                    await blobProvider.SaveBlobDataAsync(WishListController.blobContainer, guid.ToString(), compressed);

                    // Add to cache
                    using (MemoryStream ms = new MemoryStream())
                    {
                        compressed.Position = 0;
                        compressed.CopyTo(ms);
                        WishListController.imageCache.Add(guid.ToString(), Convert.ToBase64String(ms.ToArray()));
                    }

                    fs.Close();
                    br.Close();
                }

                WishItem wishItem = new WishItem
                {
                    name = provider.FormData["name"],
                    type = provider.FormData["type"],
                    brand = provider.FormData["brand"],
                    no = provider.FormData["no"],
                    comment = provider.FormData["comment"],
                    price = provider.FormData["price"] == null ? null : (int?)int.Parse(provider.FormData["price"]),
                    currency = provider.FormData["currency"],
                    status = 0,
                    imageId = guid.HasValue ? guid.ToString() : null
                };

                db.WishItem.Add(wishItem);
                db.SaveChanges();

                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Moved);
                string fullyQualifiedUrl = Request.RequestUri.GetLeftPart(UriPartial.Authority);
                response.Headers.Location = new Uri(fullyQualifiedUrl + "/#/makewish?afterAdd=1");

                return response;
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e);
            }
        }

        [HttpPost]
        public HttpResponseMessage Complete([FromUri] int id, [FromBody]string feedback)
        {
            HttpResponseMessage response = new HttpResponseMessage();
            IQueryable<WishItem> toCompleteItems = db.WishItem.Where(x => x.ID == id);

            if (toCompleteItems.Count() <= 0)
            {
                response.Content = new StringContent("Specified item id not found.");
                response.StatusCode = HttpStatusCode.NotFound;
                return response;
            }

            WishItem toCompleteItem = toCompleteItems.First();
            toCompleteItem.status = 1;
            toCompleteItem.feedback = feedback;
            db.SaveChanges();

            response.Content = new StringContent("Complete item successfully.");
            response.StatusCode = HttpStatusCode.OK;
            return response;
        }

        [HttpPost]
        public async Task<HttpResponseMessage> Delete([FromBody] int id)
        {
            HttpResponseMessage response = new HttpResponseMessage();
            IQueryable<WishItem> toDeleteItems = db.WishItem.Where(x => x.ID == id);

            if (toDeleteItems.Count() <= 0)
            {
                response.Content = new StringContent("Specified item id not found.");
                response.StatusCode = HttpStatusCode.NotFound;
                return response;
            }

            WishItem toDeleteItem = toDeleteItems.First();
            db.WishItem.Remove(toDeleteItem);
            db.SaveChanges();

            if (toDeleteItem.imageId != null)
            {
                // delete from cache
                if (WishListController.imageCache.ContainsKey(toDeleteItem.imageId))
                {
                    WishListController.imageCache.Remove(toDeleteItem.imageId);
                }

                // delete blob from azure blob storage
                await blobProvider.DeleteBlobDataAsync(WishListController.blobContainer, toDeleteItem.imageId);
            }

            response.Content = new StringContent("Delete item successfully.");
            response.StatusCode = HttpStatusCode.OK;
            return response;
        }

        /// <summary>
        /// Compresse the image.
        /// </summary>
        /// <param name="image">The image to resize.</param>
        /// <param name="quality">The compress quality.</param>
        /// <returns>The compressed image stream.</returns>
        private Stream VaryQualityLevel(Image image, long quality)
        {
            Stream compressedImageStream = new MemoryStream();

            // Get a bitmap. The using statement ensures objects  
            // are automatically disposed from memory after use.  
            using (Bitmap bmp = new Bitmap(image))
            {
                ImageCodecInfo encoder = GetEncoder(ImageFormat.Jpeg);

                // Create an Encoder object based on the GUID  
                // for the Quality parameter category.  
                Encoder myEncoder = Encoder.Quality;

                // Create an EncoderParameters object.  
                // An EncoderParameters object has an array of EncoderParameter  
                // objects. In this case, there is only one  
                // EncoderParameter object in the array.  
                EncoderParameters myEncoderParameters = new EncoderParameters(1);
                EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, quality);
                myEncoderParameters.Param[0] = myEncoderParameter;
                bmp.Save(compressedImageStream, encoder, myEncoderParameters);
                compressedImageStream.Position = 0;

                return compressedImageStream;
            }
        }

        /// <summary>
        /// Get image encoder.
        /// </summary>
        /// <param name="format">The image format.</param>
        /// <returns>Image CodecInfo.</returns>
        private ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }

            return null;
        }

        /// <summary>
        /// Resize the image to the specified width and height.
        /// </summary>
        /// <param name="image">The image to resize.</param>
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        /// <returns>The resized image.</returns>
        private Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        /// <summary>
        /// Resize the image to the specified width and height.
        /// </summary>
        /// <param name="image">The image to resize.</param>
        /// <param name="max">The max width/height size.</param>
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        private void GetImageSize(Image image, int max, out int width, out int height)
        {
            if (image.Width > image.Height)
            {
                height = (int)((double)max / image.Width * image.Height);
                width = max;
            }
            else
            {
                width = (int)((double)max / image.Height * image.Width);
                height = max;
            }
        }
    }
}
