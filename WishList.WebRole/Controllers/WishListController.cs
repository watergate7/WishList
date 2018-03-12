using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WishList.WebRole.Models;

namespace WishList.WebRole.Controllers
{
    public class WishListController : ApiController
    {
        private WishListEntitiesContext db = new WishListEntitiesContext();

        [HttpGet]
        public IEnumerable<WishItem> Get() {
            return db.WishItem.AsEnumerable<WishItem>();
        }

        [HttpPost]
        public void Add([FromBody] WishItem item) {
            db.WishItem.Add(item);
        }
    }
}
