using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using WishList.WebRole.Models;

namespace WishList.WebRole.Controllers
{
    public class WishListController : ApiController
    {
        private WishListEntitiesContext db = new WishListEntitiesContext();

        [HttpGet]
        public List<WishItemContract> Get()
        {
            db.Database.CommandTimeout = 60 * 5;
            List<WishItemContract> items = (from item in db.WishItem
                                            select new WishItemContract
                                            {
                                                ID = item.ID,
                                                name = item.name,
                                                type = item.type,
                                                brand = item.brand,
                                                no = item.no,
                                                price = item.price,
                                                blob = item.Image.blob
                                            }).ToList();

            foreach (WishItemContract item in items)
            {
                item.base64 = Convert.ToBase64String(item.blob);
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
            var provider = new MultipartFormDataStreamProvider(root);

            try
            {
                // Read the form data
                await Request.Content.ReadAsMultipartAsync(provider);

                // Get uploaded image
                Image image = null;
                if (provider.FileData.Count > 0)
                {
                    MultipartFileData file = provider.FileData[0];
                    FileStream fs = new FileStream(file.LocalFileName, FileMode.Open);
                    BinaryReader br = new BinaryReader(fs);
                    Byte[] bytes = br.ReadBytes((Int32)fs.Length);

                    image = new Image
                    {
                        blob = bytes
                    };
                    db.Image.Add(image);

                    fs.Close();
                    br.Close();
                }

                WishItem wishItem = new WishItem
                {
                    name = provider.FormData["name"],
                    type = provider.FormData["type"],
                    brand = provider.FormData["brand"],
                    no = provider.FormData["no"],
                    price = provider.FormData["price"] == null ? null : (int?)int.Parse(provider.FormData["price"])
                };

                if (image != null)
                {
                    wishItem.imageId = image.ID;
                    wishItem.Image = image;
                }

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
    }
}
