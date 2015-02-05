using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using System.IO;
using BooksInfo.Models;
using System.Net;
using System.Text;
using System.Data;
namespace BooksInfo.Controllers
{
    public class HomeController : Controller
    {
        BooksInfoEntities db = new BooksInfoEntities();

        public ActionResult Index()
        {
            ViewBag.Message = "Modify this template to jump-start your ASP.NET MVC application.";

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult BooksInfo()
        {


            List<Book> objbbb = db.Books.Where(b => b.ISBN.Equals("0")).ToList();
            return View(objbbb);
        }


        private string APICall(string isbn)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://api.saxo.com/v1/products/products.json?key=08964e27966e4ca99eb0029ac4c4c217&isbn="+isbn+"");
            try
            {
                WebResponse response = request.GetResponse();
                using (Stream responseStream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                    return reader.ReadToEnd();
                }
            }
            catch (WebException ex)
            {
                WebResponse errorResponse = ex.Response;
                using (Stream responseStream = errorResponse.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, Encoding.GetEncoding("utf-8"));
                    String errorText = reader.ReadToEnd();
                }
                throw;
            }
        }

        public void SaveChange(long id, bool value)
        {
            try
            {
            Book objBook = db.Books.SingleOrDefault(b => b.ID == id);
            if (objBook != null)
            {
                objBook.IsMarked = value;
                db.ObjectStateManager.ChangeObjectState(objBook, EntityState.Modified);
                db.SaveChanges();
            }

            }
            catch (Exception)
            {

                throw;
            }
        }

      
        public PartialViewResult CallISBNAPI(string isbn)
        {
            string ISBNforAPI = CheckForAlreadyExistingISBN(isbn);

            String apiresult = string.Empty;
            if (!string.IsNullOrEmpty(ISBNforAPI))
            {
                apiresult = APICall(ISBNforAPI);

                dynamic dynObj = JsonConvert.DeserializeObject(apiresult);

                var allvalue = dynObj.products;

                List<Book> objBooksList = new List<Book>();

                foreach (var item in allvalue)
                {
                    Book objBook = new Book();
                    objBook.Book_ID = item.id;
                    objBook.alias = item.alias;
                    objBook.Title = item.title;


                    objBook.url = item.url;
                    objBook.image_url = item.imageurl;
                    objBook.ISBN = item.isbn13;
                    objBook.ratingcount = item.ratingcount;

                    objBook.label = item.label;

                    objBook.Price = item.pris;

                    objBooksList.Add(objBook);
                    db.Books.AddObject(objBook);
                    db.SaveChanges();
                    List<Book_Description> objBookDescriptionList = new List<Book_Description>();
                    foreach (var descriptionDetails in item.descriptions)
                    {
                        Book_Description objbookDescription = new Book_Description();
                        objbookDescription.Description = descriptionDetails.description;
                        //objbookDescription.Date_Created = descriptionDetails.datecreated;
                        objbookDescription.BookID = objBook.ID;
                        objBookDescriptionList.Add(objbookDescription);
                        db.Book_Description.AddObject(objbookDescription);
                    }
                    List<Contributor> objBookContributorList = new List<Contributor>();
                    foreach (var objCon in item.contributors)
                    {
                        Contributor objContributor = new Contributor();
                        objContributor.alias = objCon.alias;
                        objContributor.BookID = objBook.ID;
                        objContributor.Name = objCon.name;
                        objContributor.Url = objCon.url;
                        objBookContributorList.Add(objContributor);
                        db.Contributors.AddObject(objContributor);
                    }

                    db.SaveChanges();
                }
            }
            var objBooks = from foo in db.Books
                       where isbn.Contains(foo.ISBN)
                       select foo;
            return PartialView("_BooksView", objBooks);
        }

        public string CheckForAlreadyExistingISBN(string isbn)
        {
            try
            {
                var isbnLocalDB = from s in db.Books
                              where isbn.Contains(s.ISBN)
                              select s.ISBN;

                var isbnList = isbn.Split(',').ToList();

                var res = isbnList.Except(isbnLocalDB).ToList();

                string resISBN = string.Empty;
                foreach (var item in res)
                {
                    if(!string.IsNullOrEmpty(item.ToString()))
                    resISBN += item + ",";
                }
                if(resISBN.Contains(","))
                resISBN= resISBN.Remove(resISBN.Length - 1, 1);

                return resISBN;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

    }
}
