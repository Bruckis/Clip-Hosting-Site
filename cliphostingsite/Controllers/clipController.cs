using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using cliphostingsite.Models;
using BC = BCrypt.Net.BCrypt;
using System.Diagnostics;

namespace cliphostingsite.Controllers
{
    public class clipController : Controller

    {

        readonly string connectionstring = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|userdb.mdf;Integrated Security=True";


        public bool isSessionValid()
        {
            if (!string.IsNullOrEmpty(Session["publicuid"] as string))
            {
                DataTable dataTable = new DataTable();
                using (SqlConnection sqlCon = new SqlConnection(connectionstring))
                {
                    sqlCon.Open();
                    string query = "SELECT publicuid FROM usertbl WHERE publicuid = @publicuid";
                    SqlDataAdapter sqlDa = new SqlDataAdapter(query, sqlCon);
                    sqlDa.SelectCommand.Parameters.AddWithValue("@publicuid", Session["publicuid"]);
                    sqlDa.Fill(dataTable);
                    sqlCon.Close();

                }
                if (dataTable.Rows.Count > 0) { return true; }
                else { Session["publicuid"] = null; return false; }
            }
            else
            {
                return false;
            }
        }
        // GET: Clip
        [HttpGet]
        public ActionResult Upload()
        {
            if (isSessionValid())
            {
                return View(new clipModel());
            }
            else
            {
                ViewBag.Message = "You need to be logged in to use this feature. Please log in";
                return RedirectToAction("Register");
            }

        }
        [HttpPost]
        public ActionResult Upload(clipModel clipModel, HttpPostedFileBase clip, userModel userModel)
        {
            if (isSessionValid() == true)
            {
                Guid g = Guid.NewGuid();
                string clipid = Convert.ToBase64String(g.ToByteArray());
                clipid = clipid.Replace("=", "");
                clipid = clipid.Replace("+", "");
                clipid = clipid.Replace("/", "");
                clipid = clipid.Replace("_", "");
                clipid = clipid.Replace("-", "");
                clipid = clipid.PadRight(8).Substring(0, 8);
                DataTable dtblClips = new DataTable();
                DateTime date = DateTime.Now;
                string publishedtime = date.ToString("yyyy-MM-dd HH:mm:ss.fff");
                if (Request.Files.Count > 0)
                {
                    var file = Request.Files[0];
                    var name = Path.GetFileName(file.FileName);
                    string filename = name.Substring(Math.Max(0, name.Length - 4));
                    if (file != null && file.ContentLength > 0)
                    {
                        var path = Path.Combine(Server.MapPath("~/Content/videos"), String.Concat(clipid, filename));
                        Session["file"] = String.Concat(clipid, filename);
                        file.SaveAs(path);
                    }
                    using (SqlConnection sqlCon = new SqlConnection(connectionstring))
                    {
                        sqlCon.Open();
                        string query = ("INSERT INTO cliptbl (clipid, createdat) VALUES(@clipid, @createdat)");
                        SqlCommand sqlCmd = new SqlCommand(query, sqlCon);
                        sqlCmd.Parameters.AddWithValue("@clipid", String.Concat(clipid, filename));
                        sqlCmd.Parameters.AddWithValue("@createdat", publishedtime);
                        sqlCmd.ExecuteReader();
                        Session["filename"] = String.Concat(clipid, filename);
                        Session["fileid"] = clipid;
                        Session["isEditor"] = "true";
                        sqlCon.Close();
                        return RedirectToAction("Edit", new { id = clipid });


                    }

                }
                else return View("Index");
            }
            else ViewBag.Message = "You must be logged in to use this feature"; return View("Login");

        }

        // GET: Clip/view/5
        [HttpGet]
        public ActionResult Edit()
        {
            string editid = Session["fileid"].ToString();
            if (editid == null)
            {
                return RedirectToAction("Index");
            }
            if (Session["fileid"].ToString() != editid || Session["isEditor"].ToString() != "true")
            {

            }
            clipModel clipModel = new clipModel();
            DataTable clipTable = new DataTable();
            using (SqlConnection sqlCon = new SqlConnection(connectionstring))
            {
                sqlCon.Open();
                string query = "SELECT title, description, clipid FROM cliptbl WHERE clipid = @clipid";
                SqlDataAdapter sqlDa = new SqlDataAdapter(query, sqlCon);
                sqlDa.SelectCommand.Parameters.AddWithValue("@clipid", editid);
                sqlDa.Fill(clipTable);
                sqlCon.Close();
            }

            if (clipTable.Rows.Count == 1)
            {
                clipModel.title = clipTable.Rows[0][0].ToString();
                clipModel.description = clipTable.Rows[0][1].ToString();
                clipModel.clipid = clipTable.Rows[0][2].ToString();
                return View(clipModel);
            }
            else return View(clipModel);
        }
        // POST: clip/Edit/5
        [HttpPost]
        public ActionResult Edit(string id, clipModel clipModel)
        {
            string connectionstring1 = connectionstring;
            string clipid = id;
            bool isChanged = false;
            using (SqlConnection sqlConn = new SqlConnection(connectionstring1))
            {
                DataTable dataTable = new DataTable();
                string sql = "SELECT clipid FROM cliptbl";
                using (var command = new SqlCommand(sql, sqlConn))
                {
                    sqlConn.Open();
                    SqlDataAdapter sqlDa = new SqlDataAdapter(sql, sqlConn);
                    sqlDa.Fill(dataTable);

                    for (int i = 0; i < dataTable.Rows.Count; i++)
                    {
                        string input = dataTable.Rows[i][0].ToString();
                        System.Diagnostics.Debug.WriteLine(dataTable.Rows[i][0].ToString());
                        string index = input.Remove(8, input.Length - 8);
                        if (index == clipid)
                        {
                            clipid = dataTable.Rows[i][0].ToString();
                            System.Diagnostics.Debug.WriteLine("clipName: " + clipid);
                            isChanged = true;
                            break;
                        }
                    }
                    if (isChanged == false)
                    {
                        Session["failedToFind"] = "true";
                    }
                    else
                    {
                        Session["failedToFind"] = "false";
                    }


                }
                if (Session["failedToFind"].ToString() == "true")
                {
                    return RedirectToAction("Index");
                }
            }



            using (SqlConnection sqlCon = new SqlConnection(connectionstring1))
            {
                string query = ("UPDATE cliptbl SET title = @title, description = @description WHERE clipid = @clipid");
                string title = Request.Form["title"].ToString();
                string description = Request.Form["description"].ToString();
                SqlCommand sqlCmd = new SqlCommand(query, sqlCon);
                sqlCmd.Parameters.AddWithValue("@title", title);
                sqlCmd.Parameters.AddWithValue("@description", description);
                sqlCmd.Parameters.AddWithValue("@clipid", clipid);
                System.Diagnostics.Debug.WriteLine("title: " + Request.Form["title"]);
                try
                {
                    sqlCon.Open();
                    Int32 rowsAffected = sqlCmd.ExecuteNonQuery();
                    sqlCon.Close();

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

            }
            return RedirectToAction("Watch", new { id });
        }
        // GET: Clip/Edit/5
        [HttpGet]
        public ActionResult Watch(string id)
        {

            string clipid = id;
            bool isChanged = false;
            using (SqlConnection sqlConn = new SqlConnection(connectionstring))
            {
                DataTable dataTable = new DataTable();
                string sql = "SELECT clipid FROM cliptbl";
                using (var command = new SqlCommand(sql, sqlConn))
                {
                    sqlConn.Open();
                    SqlDataAdapter sqlDa = new SqlDataAdapter(sql, sqlConn);
                    sqlDa.Fill(dataTable);

                    for (int i = 0; i < dataTable.Rows.Count; i++)
                    {
                        string input = dataTable.Rows[i][0].ToString();
                        System.Diagnostics.Debug.WriteLine(dataTable.Rows[i][0].ToString());
                        string index = input.Remove(8, input.Length - 8);
                        if (index == clipid)
                        {
                            clipid = dataTable.Rows[i][0].ToString();
                            System.Diagnostics.Debug.WriteLine("clipName: " + clipid);
                            isChanged = true;
                            break;
                        }
                    }
                    if (isChanged == false)
                    {
                        Session["failedToFind"] = "true";
                    }
                    else
                    {
                        Session["failedToFind"] = "false";
                    }


                }
                if (Session["failedToFind"].ToString() == "true")
                {
                    return RedirectToAction("Index");
                }
            }

            string editid = clipid;
            if (editid == null)
            {
                return RedirectToAction("Upload");
            }
            clipModel clipModel = new clipModel();
            DataTable clipTable = new DataTable();
            using (SqlConnection sqlCon = new SqlConnection(connectionstring))
            {
                sqlCon.Open();
                string query = "SELECT title, description, clipid FROM cliptbl WHERE clipid = @clipid";
                SqlDataAdapter sqlDa = new SqlDataAdapter(query, sqlCon);
                sqlDa.SelectCommand.Parameters.AddWithValue("@clipid", editid);
                sqlDa.Fill(clipTable);
                sqlCon.Close();
            }

            if (clipTable.Rows.Count == 1)
            {
                clipModel.title = clipTable.Rows[0][0].ToString();
                clipModel.description = clipTable.Rows[0][1].ToString();
                clipModel.clipid = clipTable.Rows[0][2].ToString();
                return View(clipModel);

            }
            else return View(clipModel);
        }

        [HttpGet]
        public ActionResult Login()
        {
            if (isSessionValid())
            {
                ViewBag.Message = "You are already logged in!";
                return RedirectToAction("Upload");
            }
            else
            {
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AuthUserLogin(loginModel loginModel)
        {
            if (!ModelState.IsValid)
            {
                return View(loginModel);
            }
            DataTable accountTable = new DataTable();
            using (SqlConnection sqlCon = new SqlConnection(connectionstring))
            {
                sqlCon.Open();
                string query = "SELECT username, password FROM usertbl WHERE username = @username";
                SqlDataAdapter sqlDa = new SqlDataAdapter(query, sqlCon);
                sqlDa.SelectCommand.Parameters.AddWithValue("@username", Request.Form["username"]);
                sqlDa.Fill(accountTable);
                sqlCon.Close();
            }
            if (accountTable.Rows.Count == 1)
            {
                if (!BC.Verify(accountTable.Rows[0][0].ToString(), BC.HashPassword(Request.Form["password"])))
                {
                    // authentication failed
                    ViewBag.Message = "Authentication failed, wrong password";
                    return RedirectToAction("Login");
                }
                else
                {
                    // authentication successful
                    DataTable userTable = new DataTable();
                    using (SqlConnection sqlCon = new SqlConnection(connectionstring))
                    {
                        sqlCon.Open();
                        string query = "SELECT username, avatar, publicuid FROM usertbl WHERE username = @username";
                        SqlDataAdapter sqlDa = new SqlDataAdapter(query, sqlCon);
                        sqlDa.SelectCommand.Parameters.AddWithValue("@username", Request.Form["username"]);
                        sqlDa.Fill(userTable);
                        sqlCon.Close();
                    }
                    if (userTable.Rows.Count == 1)
                    {
                        Session["username"] = userTable.Rows[0][0].ToString();
                        Session["avatar"] = userTable.Rows[0][1].ToString();
                        Session["loggedIn"] = "true";
                        Session["publicuid"] = userTable.Rows[0][2].ToString();
                        return RedirectToAction("Upload");
                    }
                    else
                    {
                        ViewBag.Message = "Error finding user, please contact support";
                        return RedirectToAction("Upload");
                    }

                }
            }
            else
            {
                ViewBag.Message = "Backend error, please contact support.";
                return RedirectToAction("Register");
            }
        }

        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RegisterUser(registerModel registerModel)
        {
            if (!ModelState.IsValid)
            {
                return View(registerModel);
            }
            DataTable accountTable = new DataTable();
            using (SqlConnection sqlCon = new SqlConnection(connectionstring))
            {
                sqlCon.Open();
                string query = "SELECT username, email, password FROM usertbl WHERE username = @username OR email = @email";
                SqlDataAdapter sqlDa = new SqlDataAdapter(query, sqlCon);
                sqlDa.SelectCommand.Parameters.AddWithValue("@username", Request.Form["username"]);
                sqlDa.SelectCommand.Parameters.AddWithValue("@email", Request.Form["email"]);
                sqlDa.Fill(accountTable);
                sqlCon.Close();
            }
            if (accountTable.Rows.Count > 0)
            {
                if (accountTable.Rows[0][0].ToString() != null && accountTable.Rows[0][1].ToString() == null)
                {
                    ViewBag.Message = "This username is taken, please pick a new one";
                    return RedirectToAction("Register");
                }
                else if (accountTable.Rows[0][0].ToString() == null && accountTable.Rows[0][1].ToString() != null)
                {
                    ViewBag.Message = "This email is already registered, please log in instead";
                    return RedirectToAction("Login");
                }
                else
                {
                    ViewBag.Message = "Unknown error occured, please contact support";
                    return RedirectToAction("Upload");
                }
            }
            else
            {
                string publicuid = "";
                Guid guid1 = Guid.NewGuid();
                Guid guid2 = Guid.NewGuid();
                Guid guid3 = Guid.NewGuid();
                Guid guid4 = Guid.NewGuid();
                Guid guid5 = Guid.NewGuid();
                publicuid = String.Concat(publicuid, guid1, guid2, guid3, guid4, guid5).ToUpper();
                using (SqlConnection sqlcon = new SqlConnection(connectionstring))
                {
                    sqlcon.Open();
                    string query = ("INSERT INTO usertbl VALUES(@username, @email, @password, @avatar, @userlevel, @privateuid, @publicuid)");
                    SqlCommand sqlCmd = new SqlCommand(query, sqlcon);
                    sqlCmd.Parameters.AddWithValue("@username", Request.Form["username"]);
                    sqlCmd.Parameters.AddWithValue("@email", Request.Form["email"]);
                    sqlCmd.Parameters.AddWithValue("@password", BC.HashPassword(Request.Form["password"]));
                    sqlCmd.Parameters.AddWithValue("@avatar", "default.png");
                    sqlCmd.Parameters.AddWithValue("@userlevel", 1);
                    sqlCmd.Parameters.AddWithValue("@privateuid", Guid.NewGuid());
                    sqlCmd.Parameters.AddWithValue("@publicuid", publicuid);
                    sqlCmd.ExecuteNonQuery();
                }
                Session["username"] = Request.Form["username"];
                Session["avatar"] = Request.Form["avatar"];
                Session["publicuid"] = publicuid;
                return RedirectToAction("Upload");
            }
        }

        [HttpGet]
        public ActionResult Profile()
        {
            if (isSessionValid())
            {
                DataTable userTable = new DataTable();
                using (SqlConnection sqlCon = new SqlConnection(connectionstring))
                {
                    sqlCon.Open();
                    string query = "SELECT username, avatar, publicuid FROM usertbl WHERE publicuid = @publicuid";
                    SqlDataAdapter sqlDa = new SqlDataAdapter(query, sqlCon);
                    sqlDa.SelectCommand.Parameters.AddWithValue("@publicuid", Session["publicuid"]);
                    sqlDa.Fill(userTable);
                    sqlCon.Close();
                }
                if (userTable.Rows.Count == 1)
                {
                    Session["username"] = userTable.Rows[0][0].ToString();
                    Session["avatar"] = userTable.Rows[0][1].ToString();
                    Session["loggedIn"] = "true";
                    Session["publicuid"] = userTable.Rows[0][2].ToString();
                    return View();
                }
                else
                {
                    ViewBag.Message = "Error finding user, please contact support";
                    return RedirectToAction("Login");
                }
            }
            else
            {
                ViewBag.Message = "No valid profile found, please log in again";
                return RedirectToAction("Login");
            }
        }

        [HttpPost]
        public void changeUsername(string username)
        {
            using (SqlConnection sqlCon = new SqlConnection(connectionstring))
            {
                string query = ("UPDATE usertbl SET username = @username WHERE publicuid = @publicuid");
                SqlCommand sqlCmd = new SqlCommand(query, sqlCon);
                sqlCmd.Parameters.AddWithValue("@username", username);
                sqlCmd.Parameters.AddWithValue("@publicuid", Session["publicuid"]);
                Debug.WriteLine(query);
                Debug.WriteLine(username);
                try
                {
                    sqlCon.Open();
                    sqlCmd.ExecuteNonQuery();
                    sqlCon.Close();
                    Debug.WriteLine("Ran code");
                    Session["username"] = username;

                }
                catch (Exception ex)
                {
                    Session["errormsg"] = ex.Message;
                }

            }
        }
    }

}
