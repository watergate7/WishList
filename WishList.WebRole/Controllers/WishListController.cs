using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
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
        private WishlistEntities db = new WishlistEntities();

        private AzureBlobStorageProvider blobProvider = new AzureBlobStorageProvider(ConfigurationManager.AppSettings["AzureBlobStorage"]);

        private const string blobContainer = "images";

        [HttpGet]
        [Route("get")]
        public async Task<List<WishItemContract>> Get()
        {
            db.Database.CommandTimeout = 60 * 1;
            List<WishItemContract> items = (from item in db.WishItem
                                            select new WishItemContract
                                            {
                                                ID = item.ID,
                                                name = item.name,
                                                type = item.type,
                                                brand = item.brand,
                                                no = item.no,
                                                comment = item.comment,
                                                price = item.price,
                                                currency = item.currency,
                                                imageId = item.imageId,
                                            }).ToList();

            foreach (WishItemContract item in items)
            {
                if (await blobProvider.ExistsAsync(WishListController.blobContainer, item.imageId))
                {
                    Stream stream = await blobProvider.GetBlobDataAsync(WishListController.blobContainer, item.imageId);
                    using (MemoryStream ms = new MemoryStream())
                    {
                        stream.CopyTo(ms);
                        item.base64 = Convert.ToBase64String(ms.ToArray());
                    }
                }
            }

            return items;
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
                Guid guid = Guid.NewGuid();
                if (provider.FileData.Count > 0)
                {
                    MultipartFileData file = provider.FileData[0];
                    FileStream fs = new FileStream(file.LocalFileName, FileMode.Open);
                    BinaryReader br = new BinaryReader(fs);
                    byte[] bytes = br.ReadBytes((Int32)fs.Length);
                    byte[] compressedBytes = VaryQualityLevel(bytes);

                    // Save blob to azure blob storage
                    MemoryStream ms = new MemoryStream(compressedBytes);
                    await blobProvider.SaveBlobDataAsync(WishListController.blobContainer, guid.ToString(), ms);

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
                    imageId = guid.ToString()
                };

                db.WishItem.Add(wishItem);
                db.SaveChanges();

                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Moved);
                string fullyQualifiedUrl = Request.RequestUri.GetLeftPart(UriPartial.Authority);
                response.Headers.Location = new Uri(fullyQualifiedUrl + "/#/makewish?afterAdd=1");

                return response;
            }
            catch (System.Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e);
            }
        }

        [HttpGet]
        [Route("delete")]
        public async Task<HttpResponseMessage> Delete(int id)
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

            // delete blob from azure blob storage
            await blobProvider.DeleteBlobDataAsync(WishListController.blobContainer, toDeleteItem.imageId);

            response.Content = new StringContent("Delete item successfully.");
            response.StatusCode = HttpStatusCode.OK;
            return response;
        }

        private byte[] VaryQualityLevel(byte[] imageBytes)
        {
            Stream compressedImageStream = new MemoryStream();
            System.Drawing.Image image = null;
            using (MemoryStream stream = new MemoryStream(imageBytes))
            {
                image = System.Drawing.Image.FromStream(stream);
            }

            // Get a bitmap. The using statement ensures objects  
            // are automatically disposed from memory after use.  
            using (Bitmap bmp = new Bitmap(image))
            {
                ImageCodecInfo encoder = GetEncoder(ImageFormat.Jpeg);

                // Create an Encoder object based on the GUID  
                // for the Quality parameter category.  
                System.Drawing.Imaging.Encoder myEncoder =
                    System.Drawing.Imaging.Encoder.Quality;

                // Create an EncoderParameters object.  
                // An EncoderParameters object has an array of EncoderParameter  
                // objects. In this case, there is only one  
                // EncoderParameter object in the array.  
                EncoderParameters myEncoderParameters = new EncoderParameters(1);
                EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, 10L);
                myEncoderParameters.Param[0] = myEncoderParameter;
                bmp.Save(compressedImageStream, encoder, myEncoderParameters);
                compressedImageStream.Position = 0;
                BinaryReader br = new BinaryReader(compressedImageStream);
                byte[] compressed = br.ReadBytes((Int32)compressedImageStream.Length);

                return compressed;
            }
        }

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
    }
}
