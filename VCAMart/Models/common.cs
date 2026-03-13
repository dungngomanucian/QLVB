using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Text.RegularExpressions;
using System.IO;
using System.Text;
using System.Net.Mail;
using System.Data.SqlClient;
using System.Collections;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
namespace RICTotal.Models
{
    public class SubjectNode
    {
        public int value { get; set; }
        public string text { get; set; }
        public int ParentId { get; set; }
        public string hierarchy { get; set; }
        public List<SubjectNode> Children { get; set; }
        public string id { get; set; }
        public string Generation { get; set; }
    }
    public class ProjectInfo
    {
        public string PRJ_Code { get; set; }
        public string PRJ_Name { get; set; }
        public string PRJ_Group { get; set; }
        public string PRJ_Process { get; set; }
        public string PRJ_Module { get; set; }
        public string PRJ_Index { get; set; }
        public int PRJ_DisplayByea { get; set; }
        public int month { get; set; }
        public string MonthName { get; set; }
        public string PRJ_GHICHU { get; set; }
        public string Title { get; set; }
    }
    public class ChartViewModel
    {
        public ChartViewModel()
        {
            Series = new List<MySeriesData>();
            Categories = new List<string>();
        }

        public List<MySeriesData> Series
        {
            get;
            private set;
        }

        public List<string> Categories
        {
            get;
            private set;
        }
    }
    public class MySeriesData
    {
        public string Name { get; set; }
        public string Stack { get; set; }

        public IEnumerable<decimal> Data { get; set; }
    }


    public class MyMenu
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ParenetId { get; set; }
        public string Url { get; set; }
    }
    public class NhomSP
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ParenetId { get; set; }
        public string Url { get; set; }
    }
    public class ProductGroup1
    {
        public string Id { get; set; }
        public string PUG_Id { get; set; }
        public string PUG_ParentId { get; set; }
    }

    public class ProductGroup
    {
        public int Id { get; set; }
        public string PUG_Id { get; set; }
        public string PUG_ParentId { get; set; }
    }

    public class NoiDungSach
    {
        public string Id { get; set; }
        public string BC_Id { get; set; }
        public string BC_ParentId { get; set; }
    }

    public class BookContent
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ParenetId { get; set; }
        public string Url { get; set; }
    }
    public class MenuRecursion
    {
        public string msg = "";
        List<MyMenu> allMenuItems;
        List<MyMenu> allMenuItemsLeft;
        List<MyMenu> allMenuItemsLeftChaomua;
        List<MyMenu> allMenuItemsRight;

        List<NhomSP> allProductDropdownTree;
        List<BookContent> allBookContentDropdownTree;
        public const string OPEN_LIST_TAG = "<ul class='dl-menu'>";
        public const string CLOSE_LIST_TAG = "</ul>";
        public const string OPEN_LIST_ITEM_TAG = "<li>";
        public const string CLOSE_LIST_ITEM_TAG = "</li>";


        public const string CHILD_OPEN_LIST_TAG = "<ul class='dl-submenu'>";
        public const string CHILD_CLOSE_LIST_TAG = "</ul>";

        public const string CHILD_OPEN_LIST_ITEM_TAG = "<li>";
        public const string CHILD_CLOSE_LIST_ITEM_TAG = "</li>";


        public  MenuRecursion()
        {
            allMenuItems = GetMenuItems();
            allMenuItemsLeft = GetMenuItemsLeft();
            allMenuItemsLeftChaomua = ChaomuaGetMenuItemsLeft();

            allProductDropdownTree = GetProductTreeView();
           // 
            allMenuItemsRight = GetMenuItemsRight(); 
        }
        public List<NhomSP> GetProductTreeView()
        {
            List<ProductGroup1> products = new List<ProductGroup1>();
            List<ProductGroup1> products1 = new List<ProductGroup1>();
            List<ProductGroup1> products2 = new List<ProductGroup1>();
            // List<MyMenu> MenuItmes = new List<MyMenu>();
            List<NhomSP> MenuItmes = new List<NhomSP>();
            DataTable dt;

            string sql = "exec sp_tblProductGroups_Get_List_Menu 1 ";
            dt = RICDB.DB.RunSQL(sql, ref msg, common.strcnn);
            for (int i = 0; i < dt.Rows.Count; i++)
            {

                string url = RICTotal.Models.common._createURLFromText(dt.Rows[i]["PUG_Name"].ToString().Trim());

                if (dt.Rows[i]["PUG_ParentId"].ToString() == "00000000-0000-0000-0000-000000000000")
                {
                   
                    //MyMenu item = new MyMenu { Id = Convert.ToInt32(dt.Rows[i]["PUG_Identify"]), Name = dt.Rows[i]["PUG_Name"].ToString(), Url = url };
                    NhomSP item = new NhomSP { Id = dt.Rows[i]["PUG_Id"].ToString(), Name = dt.Rows[i]["PUG_Name"].ToString(), ParenetId= "00000000-0000-0000-0000-000000000000", Url = url };
                    MenuItmes.Add(item);
                    ProductGroup1 p = new ProductGroup1 { Id =dt.Rows[i]["PUG_Id"].ToString(), PUG_Id = dt.Rows[i]["PUG_Id"].ToString(), PUG_ParentId = dt.Rows[i]["PUG_ParentId"].ToString() };
                    products.Add(p);
                }
            }

            //Duyet cap 2
            for (int i = 0; i < products.Count; i++)
            {
                for (int k = 0; k < dt.Rows.Count; k++)
                {
                    if (products[i].PUG_Id.ToString() == dt.Rows[k]["PUG_ParentId"].ToString())
                    {
                        string url = RICTotal.Models.common._createURLFromText(dt.Rows[k]["PUG_Name"].ToString().Trim());
                     

                       // MyMenu item1 = new MyMenu { Id = Convert.ToInt32(dt.Rows[k]["PUG_Identify"].ToString()), Name = dt.Rows[k]["PUG_Name"].ToString(), ParenetId = products[i].Id, Url = url };
                        NhomSP item1 = new NhomSP { Id =dt.Rows[k]["PUG_Id"].ToString(), Name = dt.Rows[k]["PUG_Name"].ToString(), ParenetId =products[i].Id.ToString(), Url = url };
                        MenuItmes.Add(item1);
                        ProductGroup1 p = new ProductGroup1 { Id =dt.Rows[k]["PUG_Id"].ToString(), PUG_Id = dt.Rows[k]["PUG_Id"].ToString(), PUG_ParentId = dt.Rows[k]["PUG_ParentId"].ToString() };
                        products1.Add(p);
                    }
                }
            }
            //Duyet cap 3
            for (int i = 0; i < products1.Count; i++)
            {
                for (int k = 0; k < dt.Rows.Count; k++)
                {
                    if (products1[i].PUG_Id.ToString() == dt.Rows[k]["PUG_ParentId"].ToString())
                    {
                        string url = RICTotal.Models.common._createURLFromText(dt.Rows[k]["PUG_Name"].ToString().Trim());


                        // MyMenu item1 = new MyMenu { Id = Convert.ToInt32(dt.Rows[k]["PUG_Identify"].ToString()), Name = dt.Rows[k]["PUG_Name"].ToString(), ParenetId = products[i].Id, Url = url };
                        NhomSP item2 = new NhomSP { Id = dt.Rows[k]["PUG_Id"].ToString(), Name = dt.Rows[k]["PUG_Name"].ToString(), ParenetId = products1[i].Id.ToString(), Url = url };
                        MenuItmes.Add(item2);
                       // ProductGroup1 p = new ProductGroup1 { Id = dt.Rows[k]["PUG_Id"].ToString(), PUG_Id = dt.Rows[k]["PUG_Id"].ToString(), PUG_ParentId = dt.Rows[k]["PUG_ParentId"].ToString() };
                      //  products1.Add(p);
                    }
                }
            }
            dt.Dispose();

            return MenuItmes;

        }
        public string GenerateProductDropDownTree()
        {
                string OPEN_LIST_TAG = "[";
                string CLOSE_LIST_TAG = "]";
                string OPEN_LIST_ITEM_TAG = "{";
                string CLOSE_LIST_ITEM_TAG = "},";

          
            var strBuilder = new StringBuilder();


            List<NhomSP> parentItems = (from a in allProductDropdownTree where a.ParenetId == "00000000-0000-0000-0000-000000000000" select a).ToList();
            strBuilder.Append(OPEN_LIST_TAG);

            strBuilder.Append("{");
            strBuilder.Append("text:'Gốc'" + ", value:'00000000-0000-0000-0000-000000000000'");
            strBuilder.Append("},");

            foreach (var parentcat in parentItems)
            {
                strBuilder.Append(OPEN_LIST_ITEM_TAG);
                if(parentItems.Count > 0)
                    strBuilder.Append("text:'" + parentcat.Name + "',value:'" + parentcat.Id + "',");
                else
                    strBuilder.Append("text:'" + parentcat.Name + "',value:'" + parentcat.Id + "'");

                List<NhomSP> childItems = (from a in allProductDropdownTree where a.ParenetId == parentcat.Id select a).ToList();
                if (childItems.Count > 0)
                    AddChildProduct(parentcat, strBuilder);
                strBuilder.Append(CLOSE_LIST_ITEM_TAG);
            }
            strBuilder.Append(CLOSE_LIST_TAG);
            
            return strBuilder.ToString();
        }
        private void AddChildProduct(NhomSP childItem, StringBuilder strBuilder)
        {
            string OPEN_LIST_TAG = "[";
            string CLOSE_LIST_TAG = "]";
            string OPEN_LIST_ITEM_TAG = "{";
            string CLOSE_LIST_ITEM_TAG = "},";
            strBuilder.Append("items:");
            strBuilder.Append(OPEN_LIST_TAG);
            List<NhomSP> childItems = (from a in allProductDropdownTree where a.ParenetId == childItem.Id select a).ToList();
            foreach (NhomSP cItem in childItems)
            {
                strBuilder.Append(OPEN_LIST_ITEM_TAG);
                
                strBuilder.Append("text:'" + cItem.Name + "', value:'" + cItem.Id + "',");
               

                List<NhomSP> subChilds = (from a in allProductDropdownTree where a.ParenetId == cItem.Id select a).ToList();
                if (subChilds.Count > 0)
                {
                    AddChildProduct(cItem, strBuilder);
                }
                strBuilder.Append(CLOSE_LIST_ITEM_TAG);
            }
            strBuilder.Append(CLOSE_LIST_TAG);

            /*string CHILD_OPEN_LIST_TAG = "[";
            string OPEN_LIST_ITEM_TAG = "{";
            string CLOSE_LIST_ITEM_TAG = "},";
            string CHILD_CLOSE_LIST_TAG = "],";
            strBuilder.Append("items:");
            strBuilder.Append(CHILD_OPEN_LIST_TAG);
            List<NhomSP> childItems = (from a in allProductDropdownTree where a.ParenetId == childItem.Id select a).ToList();
            foreach (NhomSP cItem in childItems)
            {
               
                strBuilder.Append(OPEN_LIST_ITEM_TAG);
                strBuilder.Append( "text:'" + cItem.Name + "', value:'" + cItem.Id + "'," );
                List<NhomSP> subChilds = (from a in allProductDropdownTree where a.ParenetId == cItem.Id select a).ToList();
                if (subChilds.Count > 0)
                {
                    AddChildProduct(cItem , strBuilder);
                }
                strBuilder.Append(CLOSE_LIST_ITEM_TAG);
            }
            strBuilder.Append(CHILD_CLOSE_LIST_TAG);*/
        }
        
        //=======TAO MENU CHO PHAN NOI DUNG SACH=================================
        public List<BookContent> GetBookContentTreeView(string ma)
        {
            List<NoiDungSach> bookcontent = new List<NoiDungSach>();
            List<NoiDungSach> bookcontent1 = new List<NoiDungSach>();
            List<NoiDungSach> bookcontent2 = new List<NoiDungSach>();
            // List<MyMenu> MenuItmes = new List<MyMenu>();
            List<BookContent> MenuItmes = new List<BookContent>();
            DataTable dt;

            string sql = "exec sp_tblBookContent_Get_List_Menu N'" + ma + "', 1";
            dt = RICDB.DB.RunSQL(sql, ref msg, common.strcnn);
            for (int i = 0; i < dt.Rows.Count; i++)
            {

                string url = RICTotal.Models.common._createURLFromText(dt.Rows[i]["BC_Title"].ToString().Trim());

                if (dt.Rows[i]["BC_ParentId"].ToString() == "00000000-0000-0000-0000-000000000000")
                {

                    //MyMenu item = new MyMenu { Id = Convert.ToInt32(dt.Rows[i]["PUG_Identify"]), Name = dt.Rows[i]["PUG_Name"].ToString(), Url = url };
                    BookContent item = new BookContent { Id = dt.Rows[i]["BC_Id"].ToString(), Name = dt.Rows[i]["BC_Title"].ToString(), ParenetId = "00000000-0000-0000-0000-000000000000", Url = url };
                    MenuItmes.Add(item);
                    NoiDungSach p = new NoiDungSach { Id = dt.Rows[i]["BC_Id"].ToString(), BC_Id = dt.Rows[i]["BC_Id"].ToString(), BC_ParentId = dt.Rows[i]["BC_ParentId"].ToString() };
                    bookcontent.Add(p);
                }
            }

            //Duyet cap 2
            for (int i = 0; i < bookcontent.Count; i++)
            {
                for (int k = 0; k < dt.Rows.Count; k++)
                {
                    if (bookcontent[i].BC_Id.ToString() == dt.Rows[k]["BC_ParentId"].ToString())
                    {
                        string url = RICTotal.Models.common._createURLFromText(dt.Rows[k]["BC_Title"].ToString().Trim());


                        // MyMenu item1 = new MyMenu { Id = Convert.ToInt32(dt.Rows[k]["PUG_Identify"].ToString()), Name = dt.Rows[k]["PUG_Name"].ToString(), ParenetId = products[i].Id, Url = url };
                        BookContent item1 = new BookContent { Id = dt.Rows[k]["BC_Id"].ToString(), Name = dt.Rows[k]["BC_Title"].ToString(), ParenetId = bookcontent[i].Id.ToString(), Url = url };
                        MenuItmes.Add(item1);
                        NoiDungSach p = new NoiDungSach { Id = dt.Rows[k]["BC_Id"].ToString(), BC_Id = dt.Rows[k]["BC_Id"].ToString(), BC_ParentId = dt.Rows[k]["BC_ParentId"].ToString() };
                        bookcontent1.Add(p);
                    }
                }
            }
            //Duyet cap 3
            for (int i = 0; i < bookcontent1.Count; i++)
            {
                for (int k = 0; k < dt.Rows.Count; k++)
                {
                    if (bookcontent1[i].BC_Id.ToString() == dt.Rows[k]["BC_ParentId"].ToString())
                    {
                        string url = RICTotal.Models.common._createURLFromText(dt.Rows[k]["BC_Title"].ToString().Trim());


                        // MyMenu item1 = new MyMenu { Id = Convert.ToInt32(dt.Rows[k]["PUG_Identify"].ToString()), Name = dt.Rows[k]["PUG_Name"].ToString(), ParenetId = products[i].Id, Url = url };
                        BookContent item2 = new BookContent { Id = dt.Rows[k]["BC_Id"].ToString(), Name = dt.Rows[k]["BC_Title"].ToString(), ParenetId = bookcontent1[i].Id.ToString(), Url = url };
                        MenuItmes.Add(item2);
                        // ProductGroup1 p = new ProductGroup1 { Id = dt.Rows[k]["PUG_Id"].ToString(), PUG_Id = dt.Rows[k]["PUG_Id"].ToString(), PUG_ParentId = dt.Rows[k]["PUG_ParentId"].ToString() };
                        //  products1.Add(p);
                    }
                }
            }
            dt.Dispose();

            return MenuItmes;

        }
        public string GenerateBookContentDropDownTree(string ma)
        {
            allBookContentDropdownTree = GetBookContentTreeView(ma);
            string OPEN_LIST_TAG = "[";
            string CLOSE_LIST_TAG = "]";
            string OPEN_LIST_ITEM_TAG = "{";
            string CLOSE_LIST_ITEM_TAG = "},";


            var strBuilder = new StringBuilder();


            List<BookContent> parentItems = (from a in allBookContentDropdownTree where a.ParenetId == "00000000-0000-0000-0000-000000000000" select a).ToList();
            strBuilder.Append(OPEN_LIST_TAG);

            strBuilder.Append("{");
            strBuilder.Append("text:'Gốc'" + ", value:'00000000-0000-0000-0000-000000000000'");
            strBuilder.Append("},");

            foreach (var parentcat in parentItems)
            {
                strBuilder.Append(OPEN_LIST_ITEM_TAG);
                if (parentItems.Count > 0)
                    strBuilder.Append("text:'" + parentcat.Name + "',value:'" + parentcat.Id + "',");
                else
                    strBuilder.Append("text:'" + parentcat.Name + "',value:'" + parentcat.Id + "'");

                List<BookContent> childItems = (from a in allBookContentDropdownTree where a.ParenetId == parentcat.Id select a).ToList();
                if (childItems.Count > 0)
                    AddChildBookContent(parentcat, strBuilder);
                strBuilder.Append(CLOSE_LIST_ITEM_TAG);
            }
            strBuilder.Append(CLOSE_LIST_TAG);

            return strBuilder.ToString();
        }
        private void AddChildBookContent(BookContent childItem, StringBuilder strBuilder)
        {
            string OPEN_LIST_TAG = "[";
            string CLOSE_LIST_TAG = "]";
            string OPEN_LIST_ITEM_TAG = "{";
            string CLOSE_LIST_ITEM_TAG = "},";
            strBuilder.Append("items:");
            strBuilder.Append(OPEN_LIST_TAG);
            List<BookContent> childItems = (from a in allBookContentDropdownTree where a.ParenetId == childItem.Id select a).ToList();
            foreach (BookContent cItem in childItems)
            {
                strBuilder.Append(OPEN_LIST_ITEM_TAG);

                strBuilder.Append("text:'" + cItem.Name + "', value:'" + cItem.Id + "',");


                List<BookContent> subChilds = (from a in allBookContentDropdownTree where a.ParenetId == cItem.Id select a).ToList();
                if (subChilds.Count > 0)
                {
                    AddChildBookContent(cItem, strBuilder);
                }
                strBuilder.Append(CLOSE_LIST_ITEM_TAG);
            }
            strBuilder.Append(CLOSE_LIST_TAG);

            
        }
        //=================================
        public List<MyMenu> GetMenuItems()
        {
            List<ProductGroup> products = new List<ProductGroup>();
            List<ProductGroup> products1 = new List<ProductGroup>();
            List<ProductGroup> products2 = new List<ProductGroup>();
            List<MyMenu> MenuItmes = new List<MyMenu>();

            /*MyMenu item1 = new MyMenu { Id = 1, Name = "Item1" };
            MyMenu item2 = new MyMenu { Id = 2, Name = "Item2" };
            MyMenu item3 = new MyMenu { Id = 3, Name = "Item2_1", ParenetId = 2 };
            MyMenu item4 = new MyMenu { Id = 4, Name = "Item2_2", ParenetId = 2 };
            MyMenu item5 = new MyMenu { Id = 5, Name = "Item2_2_1", ParenetId = 4 };
            MyMenu item6 = new MyMenu { Id = 6, Name = "Item2_2_2", ParenetId = 4 };
            MyMenu item7 = new MyMenu { Id = 7, Name = "Item2_2_1_1", ParenetId = 5 };
            MyMenu item8 = new MyMenu { Id = 8, Name = "Item1_1", ParenetId = 1 };

            MenuItmes.Add(item1);
            MenuItmes.Add(item2);
            MenuItmes.Add(item3);
            MenuItmes.Add(item4);
            MenuItmes.Add(item5);
            MenuItmes.Add(item6);
            MenuItmes.Add(item7);
            MenuItmes.Add(item8);*/

            DataTable dt;
            
            string sql = "exec sp_tblProductGroups_Get_List_Menu 1 ";
            dt = RICDB.DB.RunSQL(sql, ref msg, common.strcnn);
            for (int i = 0; i < dt.Rows.Count; i++)
            {

                string url= RICTotal.Models.common._createURLFromText(dt.Rows[i]["PUG_Name"].ToString().Trim());

                if (dt.Rows[i]["PUG_ParentId"].ToString() == "00000000-0000-0000-0000-000000000000")
                {
                    url = "/ProductList/" + dt.Rows[i]["PUG_Identify"].ToString() + "-" + url;
                    MyMenu item = new MyMenu { Id = Convert.ToInt32(dt.Rows[i]["PUG_Identify"]), Name = dt.Rows[i]["PUG_Name"].ToString(), Url=url};
                    MenuItmes.Add(item);
                    ProductGroup p = new ProductGroup {Id= Convert.ToInt32(dt.Rows[i]["PUG_Identify"]), PUG_Id= dt.Rows[i]["PUG_Id"].ToString(), PUG_ParentId= dt.Rows[i]["PUG_ParentId"].ToString() };
                    products.Add(p);
                }
            }


          
            //Duyet cap 2
            for (int i=0;i<products.Count;i++)
            {                
                for(int k=0; k<dt.Rows.Count; k++)
                {
                    if(products[i].PUG_Id.ToString() == dt.Rows[k]["PUG_ParentId"].ToString())
                    {
                        string url = RICTotal.Models.common._createURLFromText(dt.Rows[k]["PUG_Name"].ToString().Trim());
                        url = "/ProductList/" + dt.Rows[k]["PUG_Identify"].ToString() + "-" + url;

                        MyMenu item1 = new MyMenu { Id = Convert.ToInt32(dt.Rows[k]["PUG_Identify"].ToString()), Name = dt.Rows[k]["PUG_Name"].ToString(), ParenetId = products[i].Id, Url=url };
                        MenuItmes.Add(item1);
                        ProductGroup p = new ProductGroup { Id = Convert.ToInt32(dt.Rows[k]["PUG_Identify"]), PUG_Id = dt.Rows[k]["PUG_Id"].ToString(), PUG_ParentId = dt.Rows[k]["PUG_ParentId"].ToString() };
                        products1.Add(p);
                    }
                }
            }

            //Duyet cap 3
            for (int i = 0; i < products1.Count; i++)
            {
                for (int k = 0; k < dt.Rows.Count; k++)
                {
                    if (products1[i].PUG_Id.ToString() == dt.Rows[k]["PUG_ParentId"].ToString())
                    {
                        string url = RICTotal.Models.common._createURLFromText(dt.Rows[k]["PUG_Name"].ToString().Trim());
                        url = "/ProductList/" + dt.Rows[k]["PUG_Identify"].ToString() + "-" + url;

                        MyMenu item1 = new MyMenu { Id = Convert.ToInt32(dt.Rows[k]["PUG_Identify"].ToString()), Name = dt.Rows[k]["PUG_Name"].ToString(), ParenetId = products1[i].Id, Url=url };
                        MenuItmes.Add(item1);
                        ProductGroup p = new ProductGroup { Id = Convert.ToInt32(dt.Rows[k]["PUG_Identify"]), PUG_Id = dt.Rows[k]["PUG_Id"].ToString(), PUG_ParentId = dt.Rows[k]["PUG_ParentId"].ToString() };
                        products2.Add(p);
                    }
                }
            }

            dt.Dispose(); 

            return MenuItmes;

        }
        public string GenerateMenuUi()
        {
            var strBuilder = new StringBuilder();
            List<MyMenu> parentItems = (from a in allMenuItems where a.ParenetId == 0 select a).ToList();
            strBuilder.Append(OPEN_LIST_TAG);
            foreach (var parentcat in parentItems)
            {
                strBuilder.Append(OPEN_LIST_ITEM_TAG);
                strBuilder.Append("<a href='" + parentcat.Url + "'>" + parentcat.Name + "</a>");
                List<MyMenu> childItems = (from a in allMenuItems where a.ParenetId == parentcat.Id select a).ToList();
                if (childItems.Count > 0)
                    AddChildItem(parentcat, strBuilder);
                strBuilder.Append(CLOSE_LIST_ITEM_TAG);
            }
            strBuilder.Append(CLOSE_LIST_TAG);
            return strBuilder.ToString();
        }

        private void AddChildItem(MyMenu childItem, StringBuilder strBuilder)
        {
            strBuilder.Append(CHILD_OPEN_LIST_TAG);
            List<MyMenu> childItems = (from a in allMenuItems where a.ParenetId == childItem.Id select a).ToList();
            foreach (MyMenu cItem in childItems)
            {
                strBuilder.Append(OPEN_LIST_ITEM_TAG);
                strBuilder.Append("<a href='" + cItem.Url + "'>" + cItem.Name + "</a>");
                List<MyMenu> subChilds = (from a in allMenuItems where a.ParenetId == cItem.Id select a).ToList();
                if (subChilds.Count > 0)
                {
                    AddChildItem(cItem, strBuilder);
                }
                strBuilder.Append(CLOSE_LIST_ITEM_TAG);
            }
            strBuilder.Append(CHILD_CLOSE_LIST_TAG);
        }

       
        //-----------------Xu ly menu left -------------------------------------------------------------


        //-----------------Menu left
        /*public const string OPEN_LIST_TAG_LEFT = "<ul class='category-list'>";
        public const string CLOSE_LIST_TAG_LEFT = "</ul>";
        public const string OPEN_LIST_ITEM_TAG_LEFT = "<li class='dropdown side-dropdown'>";
        public const string CLOSE_LIST_ITEM_TAG_LEFT = "</li>";


        public const string CHILD_OPEN_LIST_TAG_LEFT = "<div class='custom-menu'><ul class='category-list-second'>";
        public const string CHILD_CLOSE_LIST_TAG_LEFT = "</ul></dv>";

        public const string CHILD_OPEN_LIST_ITEM_TAG_LEFT = "<li>";
        public const string CHILD_CLOSE_LIST_ITEM_TAG_LEFT = "</li>";
        */
        public const string OPEN_LIST_TAG_LEFT = "<ul id='respMenuVertical' class='ace-responsive-menu' data-menu-style='vertical'>";
        public const string CLOSE_LIST_TAG_LEFT = "</ul>";
        public const string OPEN_LIST_ITEM_TAG_LEFT = "<li>";
        public const string CLOSE_LIST_ITEM_TAG_LEFT = "</li>";


        public const string CHILD_OPEN_LIST_TAG_LEFT = "<ul class='sub-menu slide'>";
        public const string CHILD_CLOSE_LIST_TAG_LEFT = "</ul>";

        public const string CHILD_OPEN_LIST_ITEM_TAG_LEFT = "<li>";
        public const string CHILD_CLOSE_LIST_ITEM_TAG_LEFT = "</li>";
        public List<MyMenu> ChaomuaGetMenuItemsLeft()
        {
            String href = "ProductList";

            List<ProductGroup> products = new List<ProductGroup>();
            List<ProductGroup> products1 = new List<ProductGroup>();
            List<ProductGroup> products2 = new List<ProductGroup>();
            List<MyMenu> MenuItmes = new List<MyMenu>();

            /*MyMenu item1 = new MyMenu { Id = 1, Name = "Item1" };
            MyMenu item2 = new MyMenu { Id = 2, Name = "Item2" };
            MyMenu item3 = new MyMenu { Id = 3, Name = "Item2_1", ParenetId = 2 };
            MyMenu item4 = new MyMenu { Id = 4, Name = "Item2_2", ParenetId = 2 };
            MyMenu item5 = new MyMenu { Id = 5, Name = "Item2_2_1", ParenetId = 4 };
            MyMenu item6 = new MyMenu { Id = 6, Name = "Item2_2_2", ParenetId = 4 };
            MyMenu item7 = new MyMenu { Id = 7, Name = "Item2_2_1_1", ParenetId = 5 };
            MyMenu item8 = new MyMenu { Id = 8, Name = "Item1_1", ParenetId = 1 };

            MenuItmes.Add(item1);
            MenuItmes.Add(item2);
            MenuItmes.Add(item3);
            MenuItmes.Add(item4);
            MenuItmes.Add(item5);
            MenuItmes.Add(item6);
            MenuItmes.Add(item7);
            MenuItmes.Add(item8);*/

            DataTable dt;

            string sql = "exec sp_tblProductGroups_Get_List_Menu 1 ";
            dt = RICDB.DB.RunSQL(sql, ref msg, common.strcnn);
            for (int i = 0; i < dt.Rows.Count; i++)
            {

                string url = RICTotal.Models.common._createURLFromText(dt.Rows[i]["PUG_Name"].ToString().Trim());

                if (dt.Rows[i]["PUG_ParentId"].ToString() == "00000000-0000-0000-0000-000000000000")
                {
                    url = "/" + href + "/" + dt.Rows[i]["PUG_Identify"].ToString() + "-" + url + "?l=101";
                    MyMenu item = new MyMenu { Id = Convert.ToInt32(dt.Rows[i]["PUG_Identify"]), Name = dt.Rows[i]["PUG_Name"].ToString(), Url = url };
                    MenuItmes.Add(item);
                    ProductGroup p = new ProductGroup { Id = Convert.ToInt32(dt.Rows[i]["PUG_Identify"]), PUG_Id = dt.Rows[i]["PUG_Id"].ToString(), PUG_ParentId = dt.Rows[i]["PUG_ParentId"].ToString() };
                    products.Add(p);
                }
            }



            //Duyet cap 2
            for (int i = 0; i < products.Count; i++)
            {
                for (int k = 0; k < dt.Rows.Count; k++)
                {
                    if (products[i].PUG_Id.ToString() == dt.Rows[k]["PUG_ParentId"].ToString())
                    {
                        string url = RICTotal.Models.common._createURLFromText(dt.Rows[k]["PUG_Name"].ToString().Trim());
                        url = "/" + href + "/" + dt.Rows[k]["PUG_Identify"].ToString() + "-" + url + "?l=101";

                        MyMenu item1 = new MyMenu { Id = Convert.ToInt32(dt.Rows[k]["PUG_Identify"].ToString()), Name = dt.Rows[k]["PUG_Name"].ToString(), ParenetId = products[i].Id, Url = url };
                        MenuItmes.Add(item1);
                        ProductGroup p = new ProductGroup { Id = Convert.ToInt32(dt.Rows[k]["PUG_Identify"]), PUG_Id = dt.Rows[k]["PUG_Id"].ToString(), PUG_ParentId = dt.Rows[k]["PUG_ParentId"].ToString() };
                        products1.Add(p);
                    }
                }
            }

            //Duyet cap 3
            for (int i = 0; i < products1.Count; i++)
            {
                for (int k = 0; k < dt.Rows.Count; k++)
                {
                    if (products1[i].PUG_Id.ToString() == dt.Rows[k]["PUG_ParentId"].ToString())
                    {
                        string url = RICTotal.Models.common._createURLFromText(dt.Rows[k]["PUG_Name"].ToString().Trim());
                        url = "/" + href + "/" + dt.Rows[k]["PUG_Identify"].ToString() + "-" + url + "?l=101";

                        MyMenu item1 = new MyMenu { Id = Convert.ToInt32(dt.Rows[k]["PUG_Identify"].ToString()), Name = dt.Rows[k]["PUG_Name"].ToString(), ParenetId = products1[i].Id, Url = url };
                        MenuItmes.Add(item1);
                        ProductGroup p = new ProductGroup { Id = Convert.ToInt32(dt.Rows[k]["PUG_Identify"]), PUG_Id = dt.Rows[k]["PUG_Id"].ToString(), PUG_ParentId = dt.Rows[k]["PUG_ParentId"].ToString() };
                        products2.Add(p);
                    }
                }
            }

            dt.Dispose();

            return MenuItmes;

        }

        public List<MyMenu> GetMenuItemsLeft()
        {
            String href = "ProductList";
            
            List<ProductGroup> products = new List<ProductGroup>();
            List<ProductGroup> products1 = new List<ProductGroup>();
            List<ProductGroup> products2 = new List<ProductGroup>();
            List<MyMenu> MenuItmes = new List<MyMenu>();
          
            DataTable dt;

            string sql = "exec sp_tblProductGroups_Get_List_Menu 1 ";
            dt = RICDB.DB.RunSQL(sql, ref msg, common.strcnn);
            for (int i = 0; i < dt.Rows.Count; i++)
            {

                string url = RICTotal.Models.common._createURLFromText(dt.Rows[i]["PUG_Name"].ToString().Trim());

                if (dt.Rows[i]["PUG_ParentId"].ToString() == "00000000-0000-0000-0000-000000000000")
                {
                    url = "/" + href +"/" + dt.Rows[i]["PUG_Identify"].ToString() + "-" + url + "?l=101";
                    MyMenu item = new MyMenu { Id = Convert.ToInt32(dt.Rows[i]["PUG_Identify"]), Name = dt.Rows[i]["PUG_Name"].ToString(), Url = url };
                    MenuItmes.Add(item);
                    ProductGroup p = new ProductGroup { Id = Convert.ToInt32(dt.Rows[i]["PUG_Identify"]), PUG_Id = dt.Rows[i]["PUG_Id"].ToString(), PUG_ParentId = dt.Rows[i]["PUG_ParentId"].ToString() };
                    products.Add(p);
                }
            }



            //Duyet cap 2
            for (int i = 0; i < products.Count; i++)
            {
                for (int k = 0; k < dt.Rows.Count; k++)
                {
                    if (products[i].PUG_Id.ToString() == dt.Rows[k]["PUG_ParentId"].ToString())
                    {
                        string url = RICTotal.Models.common._createURLFromText(dt.Rows[k]["PUG_Name"].ToString().Trim());
                        url = "/" + href +"/" + dt.Rows[k]["PUG_Identify"].ToString() + "-" + url + "?l=101";

                        MyMenu item1 = new MyMenu { Id = Convert.ToInt32(dt.Rows[k]["PUG_Identify"].ToString()), Name = dt.Rows[k]["PUG_Name"].ToString(), ParenetId = products[i].Id, Url = url };
                        MenuItmes.Add(item1);
                        ProductGroup p = new ProductGroup { Id = Convert.ToInt32(dt.Rows[k]["PUG_Identify"]), PUG_Id = dt.Rows[k]["PUG_Id"].ToString(), PUG_ParentId = dt.Rows[k]["PUG_ParentId"].ToString() };
                        products1.Add(p);
                    }
                }
            }

            //Duyet cap 3
            for (int i = 0; i < products1.Count; i++)
            {
                for (int k = 0; k < dt.Rows.Count; k++)
                {
                    if (products1[i].PUG_Id.ToString() == dt.Rows[k]["PUG_ParentId"].ToString())
                    {
                        string url = RICTotal.Models.common._createURLFromText(dt.Rows[k]["PUG_Name"].ToString().Trim());
                        url = "/" + href +"/" + dt.Rows[k]["PUG_Identify"].ToString() + "-" + url + "?l=101";

                        MyMenu item1 = new MyMenu { Id = Convert.ToInt32(dt.Rows[k]["PUG_Identify"].ToString()), Name = dt.Rows[k]["PUG_Name"].ToString(), ParenetId = products1[i].Id, Url = url };
                        MenuItmes.Add(item1);
                        ProductGroup p = new ProductGroup { Id = Convert.ToInt32(dt.Rows[k]["PUG_Identify"]), PUG_Id = dt.Rows[k]["PUG_Id"].ToString(), PUG_ParentId = dt.Rows[k]["PUG_ParentId"].ToString() };
                        products2.Add(p);
                    }
                }
            }

            dt.Dispose();

            return MenuItmes;

        }
       //Lay thong tin ve manu left cho trang chao ban 
        public string GenerateMenuUiLeft()
        {
            var strBuilder = new StringBuilder();
            List<MyMenu> parentItems = (from a in allMenuItemsLeft where a.ParenetId == 0 select a).ToList();
            strBuilder.Append(OPEN_LIST_TAG_LEFT);

            {
                strBuilder.Append("<li>");
                strBuilder.Append("<a href='/Home'><i class='fa fa-home' aria-hidden='true'></i><span class='head1BoldWhite'>" + "DANH MỤC" + "</span></a>");
                strBuilder.Append("</li>");
            }

            foreach (var parentcat in parentItems)
            {
                
               
                strBuilder.Append(OPEN_LIST_ITEM_TAG_LEFT);
               
                List<MyMenu> childItems = (from a in allMenuItemsLeft where a.ParenetId == parentcat.Id select a).ToList();
                if (childItems.Count > 0)
                    strBuilder.Append("<a href='" + parentcat.Url + "'>" + parentcat.Name + "</a>");
                else
                    strBuilder.Append("<a href='" + parentcat.Url + "'>" + parentcat.Name + "</a>");

                if (childItems.Count > 0)
                    AddChildItemLeft(parentcat, strBuilder);
                strBuilder.Append(CLOSE_LIST_ITEM_TAG_LEFT);
            }
            strBuilder.Append(CLOSE_LIST_TAG_LEFT);
            return strBuilder.ToString();
        }

        //Lay thong tin ve manu left cho trang chao mua 
        public string GenerateMenuUiLeftChaomua()
        {
            var strBuilder = new StringBuilder();
            List<MyMenu> parentItems = (from a in allMenuItemsLeftChaomua where a.ParenetId == 0 select a).ToList();
            strBuilder.Append(OPEN_LIST_TAG_LEFT);

            {
                strBuilder.Append("<li>");
                strBuilder.Append("<a href='/Home'><i class='fa fa-home' aria-hidden='true'></i><span class='head1BoldWhite'>" + "DANH MỤC THIẾT BỊ" + "</span></a>");
                strBuilder.Append("</li>");
            }

            foreach (var parentcat in parentItems)
            {


                strBuilder.Append(OPEN_LIST_ITEM_TAG_LEFT);

                List<MyMenu> childItems = (from a in allMenuItemsLeft where a.ParenetId == parentcat.Id select a).ToList();
                if (childItems.Count > 0)
                    strBuilder.Append("<a href='" + parentcat.Url + "'>" + parentcat.Name + "</a>");
                else
                    strBuilder.Append("<a href='" + parentcat.Url + "'>" + parentcat.Name + "</a>");

                if (childItems.Count > 0)
                    AddChildItemLeft(parentcat, strBuilder);
                strBuilder.Append(CLOSE_LIST_ITEM_TAG_LEFT);
            }
            strBuilder.Append(CLOSE_LIST_TAG_LEFT);
            return strBuilder.ToString();
        }

        private void AddChildItemLeft(MyMenu childItem, StringBuilder strBuilder)
        {
            strBuilder.Append(CHILD_OPEN_LIST_TAG_LEFT);
            List<MyMenu> childItems = (from a in allMenuItemsLeft where a.ParenetId == childItem.Id select a).ToList();
            foreach (MyMenu cItem in childItems)
            {
                strBuilder.Append(OPEN_LIST_ITEM_TAG_LEFT);
                strBuilder.Append("<a  href='" + cItem.Url + "'>" + cItem.Name + "</a>");
                List<MyMenu> subChilds = (from a in allMenuItemsLeft where a.ParenetId == cItem.Id select a).ToList();
                if (subChilds.Count > 0)
                {
                    AddChildItem(cItem, strBuilder);
                }
                strBuilder.Append(CLOSE_LIST_ITEM_TAG_LEFT);
            }
            strBuilder.Append(CHILD_CLOSE_LIST_TAG_LEFT);
        }

        //-------------------Xu ly menu Right
        public List<MyMenu> GetMenuItemsRight()
        {
            String href = "ProductList";

            List<ProductGroup> products = new List<ProductGroup>();
            List<ProductGroup> products1 = new List<ProductGroup>();
            List<ProductGroup> products2 = new List<ProductGroup>();
            List<MyMenu> MenuItmes = new List<MyMenu>();

            DataTable dt;

            string sql = "exec sp_tblProductGroups_Get_List_Menu 1 ";
            dt = RICDB.DB.RunSQL(sql, ref msg, common.strcnn);
            for (int i = 0; i < dt.Rows.Count; i++)
            {

                string url = RICTotal.Models.common._createURLFromText(dt.Rows[i]["PUG_Name"].ToString().Trim());

                if (dt.Rows[i]["PUG_ParentId"].ToString() == "00000000-0000-0000-0000-000000000000")
                {
                    url = "/" + href + "/" + dt.Rows[i]["PUG_Identify"].ToString() + "-" + url + "?l=101";
                    MyMenu item = new MyMenu { Id = Convert.ToInt32(dt.Rows[i]["PUG_Identify"]), Name = dt.Rows[i]["PUG_Name"].ToString(), Url = url };
                    MenuItmes.Add(item);
                    ProductGroup p = new ProductGroup { Id = Convert.ToInt32(dt.Rows[i]["PUG_Identify"]), PUG_Id = dt.Rows[i]["PUG_Id"].ToString(), PUG_ParentId = dt.Rows[i]["PUG_ParentId"].ToString() };
                    products.Add(p);
                }
            }

            dt.Dispose();

            return MenuItmes;

        }
        public string GenerateMenuRight()
        {
            var strBuilder = new StringBuilder();
            List<MyMenu> parentItems = (from a in allMenuItemsRight where a.ParenetId == 0 select a).ToList();
            strBuilder.Append(OPEN_LIST_TAG_LEFT);

            {
                strBuilder.Append("<li>");
                strBuilder.Append("<a href='/Home'><i class='fa fa-home' aria-hidden='true'></i><span class='head1BoldWhite'>" + "DANH MỤC" + "</span></a>");
                strBuilder.Append("</li>");
            }

            foreach (var parentcat in parentItems)
            {


                strBuilder.Append(OPEN_LIST_ITEM_TAG_LEFT);

                List<MyMenu> childItems = (from a in allMenuItemsRight where a.ParenetId == parentcat.Id select a).ToList();
                if (childItems.Count > 0)
                    strBuilder.Append("<a href='" + parentcat.Url + "'>" + parentcat.Name + "</a>");
                else
                    strBuilder.Append("<a href='" + parentcat.Url + "'>" + parentcat.Name + "</a>");

                if (childItems.Count > 0)
                    AddChildItemRight(parentcat, strBuilder);
                strBuilder.Append(CLOSE_LIST_ITEM_TAG_LEFT);
            }
            strBuilder.Append(CLOSE_LIST_TAG_LEFT);
            return strBuilder.ToString();
        }
        private void AddChildItemRight(MyMenu childItem, StringBuilder strBuilder)
        {
            strBuilder.Append(CHILD_OPEN_LIST_TAG_LEFT);
            List<MyMenu> childItems = (from a in allMenuItemsRight where a.ParenetId == childItem.Id select a).ToList();
            foreach (MyMenu cItem in childItems)
            {
                strBuilder.Append(OPEN_LIST_ITEM_TAG_LEFT);
                strBuilder.Append("<a  href='" + cItem.Url + "'>" + cItem.Name + "</a>");
                List<MyMenu> subChilds = (from a in allMenuItemsRight where a.ParenetId == cItem.Id select a).ToList();
                if (subChilds.Count > 0)
                {
                    AddChildItemRight(cItem, strBuilder);
                }
                strBuilder.Append(CLOSE_LIST_ITEM_TAG_LEFT);
            }
            strBuilder.Append(CHILD_CLOSE_LIST_TAG_LEFT);
        }
    }


    public class common
    {
        public static string strcnn = System.Configuration.ConfigurationManager.AppSettings["cnn"].ToString();
        public static string userCookieTokenName = "RICTotal_Token";
        public static string MBCookieTokenName = "MB_Token";
        public static string urlHome = "Home";
        public static string app_title = "";
        public static string app_title_short = "";
        public static string app_description = "";
        public static string app_keywords = "";
        public static string msg;
        public static string strMenu;
        public static string controlURL = "";
        public common() {
            app_title = System.Configuration.ConfigurationManager.AppSettings["app_title"].ToString();
            app_title_short = System.Configuration.ConfigurationManager.AppSettings["app_title_short"].ToString();
            app_description = System.Configuration.ConfigurationManager.AppSettings["app_description"].ToString();
            app_keywords = System.Configuration.ConfigurationManager.AppSettings["app_keywords"].ToString();
        }
        public static void InitDefault()
        {
            app_title = System.Configuration.ConfigurationManager.AppSettings["app_title"].ToString();
            app_title_short = System.Configuration.ConfigurationManager.AppSettings["app_title_short"].ToString();
            app_description = System.Configuration.ConfigurationManager.AppSettings["app_description"].ToString();
            app_keywords = System.Configuration.ConfigurationManager.AppSettings["app_keywords"].ToString();

          
        }
        public string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");









        }
        //Download file
        public byte[] GetFile(string s)
        {
            System.IO.FileStream fs = System.IO.File.OpenRead(s);
            byte[] data = new byte[fs.Length];
            int br = fs.Read(data, 0, data.Length);
            if (br != fs.Length)
                throw new System.IO.IOException(s);
            return data;
        }
        public static bool Check_Permission_Inert_Edit_Delete(int Type, string uid,string id)
        {
            /*Type=1: Them; 2: Sua; 3: Xoa*/
            
            string quyen = "";
            string strcnn;
            strcnn = System.Configuration.ConfigurationManager.AppSettings["cnn"].ToString();
            string sql = "exec sp_Check_InertUpdateDelete_Member " + id +",N'" + uid + "'";
            try
            {
                DataTable dt = RICDB.DB.RunSQL(sql, ref msg, strcnn);
                if (Type == 1)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        quyen += dt.Rows[i]["NQCC_THEM"].ToString();
                    }
                }
                else if (Type == 2)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        quyen += dt.Rows[i]["NQCC_SUA"].ToString();
                    }
                }
                else if (Type == 3)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        quyen += dt.Rows[i]["NQCC_XOA"].ToString();
                    }
                }
            }
            catch 
            {
                return false;
            }
                                                                     
            if (quyen.Contains("1"))
                return true;
            else return false;
        }

        public DateTime GetFirstDateOfWeekByWeekNumber(int year, int weekNumber)
        {
            DateTime dt = new DateTime(year, 12, 31);
            CultureInfo cal = CultureInfo.CurrentCulture;
            int week = cal.Calendar.GetWeekOfYear(dt, CalendarWeekRule.FirstFullWeek, DayOfWeek.Monday);

            DayOfWeek day = DayOfWeek.Monday;
            DateTime startOfYear = new DateTime(year, 1, 1);

            int daysToFirstCorrectDay = (((int)day - (int)startOfYear.DayOfWeek) + 7) % 7;
            DateTime FirstDay = startOfYear.AddDays(7 * (weekNumber - 1) + daysToFirstCorrectDay);
            DateTime Endaday = startOfYear.AddDays(7 * (weekNumber - 1) + daysToFirstCorrectDay).AddDays(6);

            /*
            //weekNumber = 2;
            // mult by 7 to get the day number of the year
            int days = (weekNumber - 1) * 7;
            // get the date of that day
            DateTime dt1 = dt.AddDays(days);
            // check what day of week it is
            DayOfWeek dow = dt1.DayOfWeek;
            // to get the first day of that week - subtract the value of the DayOfWeek enum from the date
            DateTime startDateOfWeek = dt1.AddDays(-(int)dow);*/
            return FirstDay;
        }
        public int GetWeekNumber(DateTime dt)
        {
            CultureInfo ciCurr = CultureInfo.CurrentCulture;
            int numWeek = ciCurr.Calendar.GetWeekOfYear(dt, CalendarWeekRule.FirstFullWeek, DayOfWeek.Monday);
            return numWeek;
        }
        public int CountWeekInYear(int year)
        {

            /*DateTimeFormatInfo dfi = DateTimeFormatInfo.CurrentInfo;
            Calendar cal = DateTimeFormatInfo.CurrentInfo.Calendar;
            DateTime dt = new DateTime(year, 12, 31);
            int a = cal.GetWeekOfYear(dt, dfi.CalendarWeekRule, dfi.FirstDayOfWeek);
            return cal.GetWeekOfYear(dt, dfi.CalendarWeekRule, dfi.FirstDayOfWeek);*/
            DateTime dt = new DateTime(year, 12, 31);
            CultureInfo cal = CultureInfo.CurrentCulture;
            //GregorianCalendar cal = new GregorianCalendar(GregorianCalendarTypes.Localized);
            int week = cal.Calendar.GetWeekOfYear(dt, CalendarWeekRule.FirstFullWeek, DayOfWeek.Monday);
            return week;
        }
        public static string GetMonthName(int m)
        {
            string monthName = "";
            switch (m)
            {
                case 1:
                    monthName = "Jan";
                    break;
                case 2:
                    monthName = "Feb";
                    break;
                case 3:
                    monthName = "Mar";
                    break;
                case 4:
                    monthName = "Apr";
                    break;
                case 5:
                    monthName = "May";
                    break;
                case 6:
                    monthName = "Jun";
                    break;
                case 7:
                    monthName = "Jul";
                    break;
                case 8:
                    monthName = "Aug";
                    break;
                case 9:
                    monthName = "Sep";
                    break;
                case 10:
                    monthName = "Oct";
                    break;
                case 11:
                    monthName = "Nov";
                    break;
                case 12:
                    monthName = "Dec";
                    break;
                default:
                    monthName = "No Name";
                    break;
            }
            return monthName;
        }
        public static double stdDEV(List<double> values)
        {
            double mean = 0.0;
            double sum = 0.0;
            double stdDev = 0.0;
            int n = 0;
            foreach (double val in values)
            {
                n++;
                double delta = val - mean;
                mean += delta / n;
                sum += delta * (val - mean);
            }
            if (1 < n)
                stdDev = Math.Sqrt(sum / (n - 1));

            return stdDev;

        }
        public static void DeleteRowOutliers(ref DataTable dt, ArrayList arrFieldName)
        {

            int h = 2;
            int c = arrFieldName.Count;
            double[,] can = new double[h, c];
            //Tinh hang can tren
            for (int i = 0; i < c; i++)
            {
                string fieldName = arrFieldName[i].ToString();
                List<double> arrMoving = new List<double>();
                double mean = 0;
                for (int k = 0; k < dt.Rows.Count; k++)
                {
                    double mov = Convert.ToDouble(dt.Rows[k][fieldName]);
                    mean += Convert.ToDouble(dt.Rows[k][fieldName]);
                    arrMoving.Add(mov);
                }
                double std = Math.Round(common.stdDEV(arrMoving), 0);
                mean = Math.Round(mean / dt.Rows.Count, 0);
                double cantren = Math.Abs(std + mean);
                double canduoi = Math.Abs(mean - std);
                can[0, i] = cantren;
                can[1, i] = canduoi;
            }

            //Tim cac hang ngoai lai

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                

                /* for (int j =0; j< c; j++)
                 {
                     string fieldName = arrFieldName[j].ToString();
                     double mov = Math.Round(Convert.ToDouble(dt.Rows[i][fieldName]), 0);
                     if (mov < can[0, j] && mov > can[1, j]) { flag = true; break; }
                     else { flag =false; }
                 }
                 if (!flag) dt.Rows[i].Delete();
                 else*/
                {
                    int dem = 0;
                    for (int j = 0; j < c; j++)
                    {
                        string fieldName = arrFieldName[j].ToString();
                        double mov = Math.Round(Convert.ToDouble(dt.Rows[i][fieldName]), 0);
                        if (mov == 0) dem++;
                    }
                    if (dem >= 2) dt.Rows[i].Delete();
                }

            }

            dt.AcceptChanges();

            /* for(int i=0; i< arrFieldName.Count; i++)
             {
                 string fieldName = arrFieldName[i].ToString();
                 List<double> arrMoving = new List<double>();
                 double mean = 0;
                 for (int k = 0; k < dt.Rows.Count; k++)
                 {
                     double mov = Convert.ToDouble(dt.Rows[k][fieldName]);
                     mean += Convert.ToDouble(dt.Rows[k][fieldName]);
                     arrMoving.Add(mov);
                 }
                 double std = Math.Round(common.stdDEV(arrMoving), 0);
                 mean = Math.Round(mean / dt.Rows.Count, 0);
                 double cantren = std + mean;
                 double canduoi = mean - std;                 

             }


            /* for (int k = 0; k < dt.Rows.Count; k++)
             {
                 double mov = Math.Round(Convert.ToDouble(dt.Rows[k][fieldName]), 0);
                 if (mov > cantren)
                     dt.Rows[k].Delete();
                 if (mov < canduoi)
                     dt.Rows[k].Delete();

             }
             dt.AcceptChanges();*/
        }
        public bool check_code(string tableName, string codeName, string code)
        {
            bool b = false;
            string strSQL = "Select * From " + tableName + " Where " + codeName + "='" + code + "'";
            DataTable dt = new DataTable();
            DataSet ds = new DataSet();
            
            SqlConnection conn = new SqlConnection(strcnn);
            try
            {
                conn.Open();
                SqlDataAdapter da = new SqlDataAdapter(strSQL, conn);
                da.Fill(ds, "data");
                dt = ds.Tables[0];
                if (dt.Rows.Count > 0)
                    b = true;
                else
                    b = false;
                dt.Dispose();
                conn.Close();
                conn.Dispose();
            }
            catch
            {
            }
            return b;
        }
        public bool check_code_by_PB(string tableName, string codeName, string code, string PB_Name, string PB_Id)
        {
            bool b = false;
            string strSQL = "Select * From " + tableName + " Where " + codeName + "='" + code + "' and " + PB_Name + "=N'" + PB_Id + "'";
            DataTable dt = new DataTable();
            DataSet ds = new DataSet();

            SqlConnection conn = new SqlConnection(strcnn);
            try
            {
                conn.Open();
                SqlDataAdapter da = new SqlDataAdapter(strSQL, conn);
                da.Fill(ds, "data");
                dt = ds.Tables[0];
                if (dt.Rows.Count > 0)
                    b = true;
                else
                    b = false;
                dt.Dispose();
                conn.Close();
                conn.Dispose();
            }
            catch
            {
            }
            return b;
        }
        public static string GUIDRoot(string sid)
        {
            try
            {
                return sid.Substring(0, 8) + "-" + sid.Substring(8, 4) + "-" + sid.Substring(12, 4) + "-" + sid.Substring(16, 4) + "-" + sid.Substring(20);
            }
            catch  
            {
                return Guid.Empty.ToString();
            }
            //return sid.Substring(0, 8) + "-" + sid.Substring(8, 4) + "-" + sid.Substring(12, 4) + "-" + sid.Substring(16, 4) + "-" + sid.Substring(20);
        }


        public static bool IsEmail(string s)
        {
            ///^ ([\w -\.] +)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$/;
            Regex rx = new Regex(
        @"^[-!#$%&'*+/0-9=?A-Z^_a-z{|}~](\.?[-!#$%&'*+/0-9=?A-Z^_a-z{|}~])*@[a-zA-Z](-?[a-zA-Z0-9])*(\.[a-zA-Z](-?[a-zA-Z0-9])*)+$");
            return rx.IsMatch(s);
        }

        public static int countWordInString(string input, char c)
        {
            int d = 0;
            try
            {
                if (input == null)
                    d = 0;
                else if (input == "")
                    d = 0;
                else
                {
                    string[] a = input.Split(',');
                    int n = a.Length;
                    bool[] b;
                    b = new bool[n];
                    for (int i = 0; i < n; i++) b[i] = true;
                    int sosanpham = 0;
                    for (int i = 0; i < n; i++)
                    {
                        if (b[i])
                        {
                            sosanpham++;
                            for (int j = i + 1; j < n; j++)
                                if (a[j] == a[i]) b[j] = false;
                        }
                    }
                    d = sosanpham;
                }                
            }
            catch  { }

            return d;
            
        }
        public static string ChuanHoaXau(string s, string pass = "")
        {
            s =  s.Replace("'", "").Replace("<script", "").Replace("</script>", "").Replace("<","").Replace(">","");
            if (pass != "/") s = s.Replace("/", "");

            return s;
        }
        /// <summary>
        /// Run SQL 
        /// </summary>
        /// <param name="sql">SqlCommandText</param>
        /// <returns>Json String</returns>
        public static string RunSQLToJson(string sql, ref string msg)
        {
            System.Data.DataTable dt;
            string strcnn = common.GetAppSetting("cnn");
            dt = RICDB.DB.RunSQL(sql, ref msg, strcnn);
            if (msg != "100") return "";
            if (dt.Rows.Count == 0) return "";
            return JsonConvert.SerializeObject(dt); //, Formatting.Indented);
        }
        public static string RunSQLToJson(DataTable dt)
        {
            if (dt == null) return "";
            if (dt.Rows.Count <= 0) return "";            
            return JsonConvert.SerializeObject(dt); //, Formatting.Indented);
        }
        public static bool IsNumericType(object o)
        {
            switch (Type.GetTypeCode(o.GetType()))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// thuc hien sql tra ve json, 1 so truong object de nguyen
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="objcols">cac truong object,ko xu ly, format: ,f1,f2,f3,</param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static string RunSQLToJsonManual(string sql, string objcols,  ref string msg)
        {
            // xu ly lay ra json ca cac truong object 
            System.Data.DataTable dt;
            string strcnn = common.GetAppSetting("cnn");
            dt = RICDB.DB.RunSQL(sql, ref msg, strcnn);

            if (msg != "100") return "";
            if (dt.Rows.Count == 0) return "";

            string data = "",   row = "";
            objcols += ";";
            foreach (DataRow r in dt.Rows)
            {
                data += (data == "" ? "" : ",") + "{";
                row = "";
                foreach (DataColumn c in dt.Columns)
                {
                    if (objcols.ToLower().IndexOf(";"+ c.ColumnName.ToString().ToLower() + ";") >= 0)
                    {
                       row += (row==""?"":",") + "\"" + c.ColumnName + "\":" +( r[c.ColumnName].ToString().Trim() ==""?"[]": r[c.ColumnName].ToString().Trim()) + "";
                    }
                    else if ( IsNumericType(r[c]))
                        row += (row == "" ? "" : ",") + "\"" + c.ColumnName + "\":" + r[c.ColumnName].ToString() + "" ;
                    else
                        row += (row == "" ? "" : ",") + "\"" + c.ColumnName + "\":\"" + r[c.ColumnName].ToString() + "\"";
                }
                data += row + "}";
            }
            return "[" +  data + "]"; //, Formatting.Indented);
        }
        
        public static void RunSQLNoReturn(string sql, ref string msg, string strcnn, ref int irow)
        {
            irow = 0;
            System.Data.SqlClient.SqlCommand  cmd = new System.Data.SqlClient.SqlCommand(sql, new System.Data.SqlClient.SqlConnection(strcnn));
            cmd.CommandType = CommandType.Text;
            try
            {
                cmd.Connection.Open();
                irow = cmd.ExecuteNonQuery();
                cmd.Connection.Close();
                cmd.Connection.Dispose();
                cmd.Dispose();
                msg = "100";
            }
            catch (Exception e1)
            {
                msg = e1.Message;
            }
        }

        public static string GetParentMenus()
        { // get danh sach menu cha

            string msg = "";
            string sql = "exec [sp_tblSysMenu_Get_List_Parent]  '" + System.Web.HttpContext.Current.Session["user_id"].ToString() + "'  ";
            return RunSQLToJson(sql, ref msg);

        }
        
        public static string GetPermisionGroups()
        { // get nhom quyen de dua vao user chon

            string msg = "";
            string sql = "exec sp_tblNhomQuyen_Get_List_2Select null, '" + System.Web.HttpContext.Current.Session["user_id"].ToString() + "','" + System.Web.HttpContext.Current.Session["rc_id"].ToString() + "'";
            return RunSQLToJson(sql, ref msg);

        }
        public static string GetAllUser()
        { // get nhom quyen de dua vao user chon

            string msg = "";
            string sql = "exec sp_tblUser_Get_List_2Select  ";//, '" + System.Web.HttpContext.Current.Session["user_id"].ToString() + "','" + System.Web.HttpContext.Current.Session["rc_id"].ToString() + "'";
            return RunSQLToJson(sql, ref msg);

        }
        public static string GetTinh()
        {
            string msg="";
            string sql = "exec sp_tblTinh_GetList N'VN'";
            return RunSQLToJson(sql, ref msg);
        }
        //--------------
       
        public static bool checkPermissionOfMember(string MBId, string mnuUrl, ref string msg)
        {
            // check xem nguoi dung co duoc dung menu hay ko
            string sql;

            string strcnn = common.GetAppSetting("cnn");
            msg = "";
            sql = "exec [sp_tblMembers_Check_Quyen] '" + MBId + "',N'" + mnuUrl + "'";
            sql = RICDB.DB.RunSQLReturn1Value(sql, ref msg, strcnn);

            if (msg != "100")
            {
                msg = "{\"msg\":\"" + msg + "\"}";
                return false;
            }
            if (sql.Trim() == "0")
            {
                msg = "{\"msg\":\"Have no using permision.\"}";
                return false;
            }
            msg = "{\"msg\":\"100\"}";
            return true;
        }
        public static bool checkQuyenSudung(string userId, string mnuUrl, ref string msg )
        {
            // check xem nguoi dung co duoc dung menu hay ko
            string sql ;
             
            string strcnn = common.GetAppSetting("cnn");
            msg = "";
            sql = "exec [sp_tblUsers_Check_Quyen] '" + userId + "',N'" + mnuUrl +"'";
            sql  = RICDB.DB.RunSQLReturn1Value(sql, ref msg, strcnn);

            if (msg != "100")
            {
                msg = "{\"msg\":\"" + msg + "\"}" ;
                return false;
            }
            if (sql.Trim() == "0")
            {
                msg = "{\"msg\":\"Have no using permision.\"}";
                return false;
            }
            msg = "{\"msg\":\"100\"}";
            return true;
        }
        //Tao menu tu bang loai san pham
        public static string GetMenuProduct(string lg_id)
        {
            strMenu = "";
            int PUG_Identify = 0;
            DataTable dt;
            controlURL = "productList";
            string sql = "exec sp_tblProductGroups_Get_List_Menu " + lg_id;

            strMenu = "<ul class='dl-menu'>";

            dt = RICDB.DB.RunSQL(sql, ref msg, strcnn);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
               
                PUG_Identify = Convert.ToInt32(dt.Rows[i]["PUG_Identify"]);
                string url = RICTotal.Models.common._createURLFromText(dt.Rows[i]["PUG_Name"].ToString().Trim());

                if (dt.Rows[i]["PUG_ParentId"].ToString() == "00000000-0000-0000-0000-000000000000")
                {
                    string s = "exec sp_tblProductGroups_Get_PerentList '" + dt.Rows[i]["PUG_Id"].ToString() + "'";
                    DataTable dt1 = RICDB.DB.RunSQL(s, ref msg, strcnn);

                    if (dt1.Rows.Count > 0)
                    {
                        strMenu += "<li>";
                        strMenu += "<a href='#'>" + dt.Rows[i]["PUG_Name"].ToString() + "</a>";
                        strMenu += "<ul class='dl-submenu'>";
                    }
                    else
                    {                      
                        strMenu += "<li><a href='/" + controlURL + "/" + PUG_Identify + "-" + url + "'>" + dt.Rows[i]["PUG_Name"].ToString() + "</a></li>";                   
                    }

                    string value = dt.Rows[i]["PUG_Id"].ToString();
                    FillChildMenuProduct(value);
                    dt1.Dispose();
                }
                else
                {

                }
            }
            strMenu += "</ul>";
            return strMenu;
        }
        public static int FillChildMenuProduct(string IID)
        {
            int PUG_Identify = 0;

            string sql = "exec sp_tblProductGroups_Get_PerentList '" + IID + "'";

           

            DataTable dt = RICDB.DB.RunSQL(sql, ref msg, strcnn);
            if (dt.Rows.Count > 0)
            {

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    
                    PUG_Identify = Convert.ToInt32(dt.Rows[i]["PUG_Identify"]);
                    strMenu += "<li><a href='/" + controlURL + "/" + PUG_Identify + "'>" + dt.Rows[i]["PUG_Name"].ToString() + "</a></li>";
                    string temp = dt.Rows[i]["PUG_Id"].ToString();
                    FillChildMenuLeft(temp);
                }
                strMenu += "</ul>";
               
            }

            strMenu += "</li>";
            return 0;
        }
        //----------------------------------

        //Lay thong tin thuc don left
        public static string GetMenuLeft(int Position, string lg_id)
        {
            strMenu = "";
            int CC_Identify = 0;
            DataTable dt;

            string sql = "exec sp_tblContentCategory_Get_List_MenuLeft " + Position + "," + lg_id;


            dt = RICDB.DB.RunSQL(sql, ref msg, strcnn);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                controlURL = dt.Rows[i]["CC_FriendlyUrl"].ToString().Trim();
                CC_Identify = Convert.ToInt32(dt.Rows[i]["CC_Identify"]);

                if (dt.Rows[i]["CC_ParentId"].ToString() == "00000000-0000-0000-0000-000000000000")
                {
                    string s = "exec sp_tblContentCategory_Get_PerentList '" + dt.Rows[i]["CC_Id"].ToString() + "'";
                    DataTable dt1 = RICDB.DB.RunSQL(s, ref msg, strcnn);
                    if (dt1.Rows.Count > 0)
                    {
                        //strMenu += "<li class='dropdown side-dropdown'>";
                        //strMenu += "<a class='dropdown-toggle' data-toggle='dropdown' aria-expanded='true' href='javascript:;'>" + dt.Rows[i]["CC_Name"].ToString() + "<i class='fa fa-angle-right'></i></a>";
                        strMenu += "<li>";
                        strMenu += "<a href='javascript:;'>" + dt.Rows[i]["CC_Name"].ToString() + "</a>";
                    }
                    else
                    {
                        if (dt.Rows[i]["CC_Order"].ToString() == "1")
                            strMenu += "<li><a href='/" + controlURL + "/" + CC_Identify + "'>" + dt.Rows[i]["CC_Name"].ToString() + "</a></li>";
                        else
                            strMenu += "<li><a  href='/" + controlURL + "/" + CC_Identify + "'>" + dt.Rows[i]["CC_Name"].ToString() + "</a></li>";

                        /* if (dt.Rows[i]["CC_Order"].ToString() == "1")
                             strMenu += "<li class='dropdown side-dropdown'><a class='dropdown-toggle' data-toggle='dropdown' aria-expanded='true'  href='/" + controlURL + "/" + CC_Identify + "'>" + dt.Rows[i]["CC_Name"].ToString() + "</a></li>";
                         else
                             strMenu += "<li class='dropdown side-dropdown' ><a class='dropdown-toggle' data-toggle='dropdown' aria-expanded='true' href='/" + controlURL + "/" + CC_Identify + "'>" + dt.Rows[i]["CC_Name"].ToString() + "</a></li>";
                         */
                    }

                    string value = dt.Rows[i]["CC_Id"].ToString();
                    FillChildMenuLeft(value);
                    dt1.Dispose();
                }
                else
                {

                }
            }

            return strMenu;
        }
        public static int FillChildMenuLeft(string IID)
        {
            int CC_Identify = 0;

            string sql = "exec sp_tblContentCategory_Get_PerentList '" + IID + "'";

            DataTable dt = RICDB.DB.RunSQL(sql, ref msg, strcnn);
            if (dt.Rows.Count > 0)
            {

                strMenu += "<ul>";
                // strMenu += "<ul class='category-list-second'>";
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    controlURL = dt.Rows[i]["CC_FriendlyUrl"].ToString().Trim();
                    CC_Identify = Convert.ToInt32(dt.Rows[i]["CC_Identify"]);
                    strMenu += "<li><a href='/" + controlURL + "/" + CC_Identify + "'>" + dt.Rows[i]["CC_Name"].ToString() + "</a></li>";
                    string temp = dt.Rows[i]["CC_Id"].ToString();
                    FillChildMenuLeft(temp);
                }
                strMenu += "</ul>";
                strMenu += "</li>";
            }


            return 0;
        }

        //----------------Doc thong tin tu tep loai tin tuc va hinh thanh menu
        public static string GetMenuTop(int Position, string lg_id)
        {
          
            strMenu = "";
            int CC_Identify = 0;
            DataTable dt;
            DataTable dtProduct;
            string sql = "exec sp_tblContentCategory_Get_List_Top_Menu " + Position + "," + lg_id;
            string PUG_ParentId = "";

            try
            {
                dt = RICDB.DB.RunSQL(sql, ref msg, strcnn);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    controlURL = dt.Rows[i]["CC_FriendlyUrl"].ToString().Trim();
                    CC_Identify = Convert.ToInt32(dt.Rows[i]["CC_Identify"]);
                    string url = RICTotal.Models.common._createURLFromText(dt.Rows[i]["CC_Name"].ToString().Trim().ToLower());

                    if (dt.Rows[i]["CC_ParentId"].ToString() == "00000000-0000-0000-0000-000000000000")
                    {

                        string s = "exec Admin.sp_tblContentCategory_CountRecord '" + dt.Rows[i]["CC_Id"].ToString() + "'";
                        string strCount = RICDB.DB.RunSQLReturn1Value(s, ref msg, strcnn);
                        int count = Convert.ToInt32(strCount);

                        if (count > 0 || Convert.ToInt32(dt.Rows[i]["CC_Order"]) == 2) // dt1.Rows.Count > 0)
                        {

                            strMenu += "<li>";
                            strMenu += "<a href='#'>" + dt.Rows[i]["CC_Name"].ToString() + "</a>";

                            if (Convert.ToInt32(dt.Rows[i]["CC_Order"]) == 2)
                            {
                                string strproduct = "exec sp_tblProductGroups_Get_List_Menu " + lg_id;
                                dtProduct = RICDB.DB.RunSQL(strproduct, ref msg, strcnn);
                                strMenu += "<ul class='sub-menu' >";
                                for (int k = 0; k < dtProduct.Rows.Count; k++)
                                {
                                    int PUG_Identify = Convert.ToInt32(dtProduct.Rows[k]["PUG_Identify"]);
                                    string url1 = RICTotal.Models.common._createURLFromText(dtProduct.Rows[k]["PUG_Name"].ToString().Trim().ToLower());

                                    PUG_ParentId = dtProduct.Rows[k]["PUG_Id"].ToString().ToUpper();
                                    if (dtProduct.Rows[k]["PUG_ParentId"].ToString() == "00000000-0000-0000-0000-000000000000")
                                    {
                                        strMenu += "<li>";
                                        strMenu += "<a href='/ProductList/" + PUG_Identify + "-" + url1 + "'>" + dtProduct.Rows[k]["PUG_Name"].ToString() + "</a>";
                                        strMenu += "<ul class='sub-menu' >";
                                        for (int j = 0; j < dtProduct.Rows.Count; j++)
                                        {
                                            int PUG_id = Convert.ToInt32(dtProduct.Rows[j]["PUG_Identify"]);
                                            if (dtProduct.Rows[j]["PUG_ParentId"].ToString().ToUpper() == PUG_ParentId.ToUpper())
                                            {

                                                strMenu += "<li><a href='/ProductList/" + PUG_id + "-" + url1 + "'>" + dtProduct.Rows[j]["PUG_Name"].ToString() + "</a></li>";

                                            }
                                        }
                                        strMenu += "</ul>";

                                        strMenu += "</li>";
                                    }

                                }
                                dtProduct.Dispose();
                                strMenu += "</ul>";


                            }
                        }
                        else
                        {
                            if (dt.Rows[i]["CC_Order"].ToString() == "1")
                                strMenu += "<li class='btn active'><a href='/" + controlURL + "/" + CC_Identify + "-" + url + "'>" + dt.Rows[i]["CC_Name"].ToString() + "</a></li>";
                            else
                                strMenu += "<li ><a href='/" + controlURL + "/" + CC_Identify + "-" + url + "'>" + dt.Rows[i]["CC_Name"].ToString() + "</a></li>";
                        }


                        string value = dt.Rows[i]["CC_Id"].ToString();
                        FillChild(value);

                        // DataTable dt1 = RICDB.DB.RunSQL(s, ref msg, strcnn);


                        // dt1.Dispose();
                    }
                    else
                    {

                    }


                }
            }
            catch
            {
                return "";
            }
            /* strMenu+="<li><form class='search-form' role='search'>";
             strMenu += "<div class='form-group pull-right' id='search'>";
             strMenu += "<input type='text' class='form-control' placeholder='Search'>";
             strMenu += "<button type='submit' class='form-control form-control-submit'>Submit</button>";
             strMenu += "<a href=''><span class='search-label'><i class='glyphicon glyphicon-search'></i></span></a>";
             strMenu += "</div>";
             strMenu += "</form></li>";*/

          //  strMenu += "<li style='margin-left:50px;'><a href = 'javascript:displaySearch();' ><i  style='color:#ffffff' class='glyphicon glyphicon-search'></i></span></ a ></li>";
            return strMenu;
        }
        public static int FillChild(string IID)
        {
            int CC_Identify = 0;

            string sql = "exec Admin.sp_tblContentCategory_Get_PerentList '" + IID + "'";
            // return common.RunSQLToJson(sql);
            DataTable dt = RICDB.DB.RunSQL(sql, ref msg, strcnn);
            if (dt.Rows.Count > 0)
            {

               // strMenu += "<div class='custom-menu'>";
                strMenu += "<ul>";
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string url = RICTotal.Models.common._createURLFromText(dt.Rows[i]["CC_Name"].ToString().Trim().ToLower());
                    controlURL = dt.Rows[i]["CC_FriendlyUrl"].ToString().Trim();
                    CC_Identify = Convert.ToInt32(dt.Rows[i]["CC_Identify"]);
                    strMenu += "<li><a href='/" + controlURL + "/" + CC_Identify + "-" + url + "'>" + dt.Rows[i]["CC_Name"].ToString() + "</a></li>";
                    string temp = dt.Rows[i]["CC_Id"].ToString();
                    FillChild(temp);
                }
                strMenu += "</ul>";
               
                strMenu += "</li>";
            }


            return 0;
        }

        public static int FillChildProduct(string IID)
        {
            int PUG_Identify = 0;

            string sql = "exec sp_tblProductGroups_Get_PerentList '" + IID + "'";
            // return common.RunSQLToJson(sql);
            DataTable dt = RICDB.DB.RunSQL(sql, ref msg, strcnn);
            if (dt.Rows.Count > 0)
            {

                // strMenu += "<div class='custom-menu'>";
                strMenu += "<ul>";
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string url = RICTotal.Models.common._createURLFromText(dt.Rows[i]["PUG_Name"].ToString().Trim().ToLower());
                    controlURL = "/ProductList";
                    PUG_Identify = Convert.ToInt32(dt.Rows[i]["PUG_Identify"]);
                    strMenu += "<li><a href='/" + controlURL + "/" + PUG_Identify + "-" + url + "'>" + dt.Rows[i]["PUG_Name"].ToString() + "</a></li>";
                    string temp = dt.Rows[i]["PUG_Id"].ToString();
                    FillChildProduct(temp);
                }
                strMenu += "</ul>";

                strMenu += "</li>";
            }


            return 0;
        }


        public static string GetMenuAdmin(string userId)
        {
            // may menu -- fill ra dang html
            string sql, msg="";
            System.Data.DataTable dt;
            string strcnn = System.Configuration.ConfigurationManager.AppSettings["cnn"].ToString();

            string mnu = "";
            sql = "exec [sp_get_menu_list] '" + userId+ "'";
            dt = RICDB.DB.RunSQL(sql, ref msg, strcnn);
            if (msg != "100") return "";
            if (dt.Rows.Count == 0) return "";

            int i;
            int mn_tt;
            string muc = dt.Rows[0]["mn_muc"].ToString();
            int dau = 0;
            int j = 0;
            for (i = 0; i < dt.Rows.Count; i++)
            {
                // truong hop la parent 
                mn_tt = Convert.ToInt32(dt.Rows[i]["mn_tt"].ToString());
                if (dt.Rows[i]["mn_muc"].ToString() == "0")
                {
                    if (dau != 0)
                    {
                        dau = 1;
                        mnu += "</li>";
                    }
                    if (dt.Rows[i]["mn_havechild"].ToString() == "0")
                    { // ko co child => link truc tiep
                        mnu += "<li>";
                        mnu += "          <a href = '/" + dt.Rows[i]["mn_url"].ToString() +  "' class='" + (dt.Rows[i]["mn_url"].ToString()=="Bill/21" || dt.Rows[i]["mn_url"].ToString() == "POS" ? "pos_button' target='_blank":"") + "'>";
                        mnu += "              <i class='mnu_parent " + dt.Rows[i]["mn_icon"].ToString() + "'></i> <span> " + dt.Rows[i]["mn_ten"].ToString() + "</span>";
                        mnu += "          </a>";
                    }
                    else
                    {
                        mnu += "<li class='treeview menu-open'>";
                        mnu += "          <a href = '/SubMenus/" + dt.Rows[i]["mn_url"].ToString() + "' >";
                        mnu += "              <i class='mnu_parent " + dt.Rows[i]["mn_icon"].ToString() + "'></i> <span> " + dt.Rows[i]["mn_ten"].ToString() + "</span>";
                        mnu += "              <span class='pull-right-container'>";
                        mnu += "                  <i class='fa fa-angle-left pull-right'></i>";
                        mnu += "              </span>";
                        mnu += "          </a>";
                    }

                }
                else // la con - muc1
                {
                    // duyet cac con cua 1 muc
                    mnu += "  <ul class='treeview-menu' style='display:block'> "; // bat dau menu con
                    for (j = i ; j < dt.Rows.Count; j++)
                    {
                        mn_tt = Convert.ToInt32(dt.Rows[j]["mn_tt"].ToString());
                        if (dt.Rows[j]["mn_muc"].ToString() == "0") break;
                        // submenu item
                        mnu += "             <li><a href = '/" + dt.Rows[j]["mn_url"].ToString() + "/" + mn_tt  + "' ><i  class='mnu_child " + dt.Rows[j]["mn_icon"].ToString() + "'></i> " + dt.Rows[j]["mn_ten"].ToString() + "</a></li>";
                    }
                    mnu += "  </ul>"; // cua sub menu
                    i = j - 1; // de chay tiep
                }

            }
            if (mnu != "") mnu += "</li>"; // end menu cha

      // '     mnu += "<li><a href=#><i class='mnu_parent fa fa-edit'></i> <span>VAT Điện tử</span></a></li>";
            return mnu;
   }

        public static string GetSubMenus(string userId, string parentURL)
        {
            // may menu -- fill ra dang html
            string sql, msg = "";
            System.Data.DataTable dt;
            string strcnn = System.Configuration.ConfigurationManager.AppSettings["cnn"].ToString();

            string mnu = "";
            sql = "exec [sp_get_menu_list_sub] '" + System.Web.HttpContext.Current.Session["rc_id"].ToString() + "','" + userId + "','" + parentURL + "'";
            dt = RICDB.DB.RunSQL(sql, ref msg, strcnn);
            if (msg != "100") return "";
            if (dt.Rows.Count == 0) return "";

            int i;

            for (i = 0; i < dt.Rows.Count; i++)
            {
                    mnu += "<li><a target=_blank href = '/" + dt.Rows[i]["mn_url"].ToString() + "' ><div><i style='width:30px;' class=' " + dt.Rows[i]["mn_icon"].ToString() + "'></i> " + dt.Rows[i]["mn_ten"].ToString() + "</div></a></li>";
            }
           
            return "<ul>" + mnu + "</ul>";
        }

        public int code;

        public static string GetAppSetting(string key)
        {
            try
            {
                return System.Configuration.ConfigurationManager.AppSettings[key].ToString();
            }
            catch  
            {
                return "";
            }
        }


        /// <summary>
        /// Hàm check xem 1 đối tượng có đủ các thuộc tính hay ko, nếu ko đủ => tra về false và thuộc tính thiếu trong key
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="skeys"></param>
        /// <returns></returns>
        public static bool CheckObjectEnoughProperies(dynamic obj, string skeys, ref string key)
        {
            key = "";
            string[] s = skeys.Split(';');
            int i;
            for (i = 0; i < s.Length; i++)
            {
                s[i] = s[i].Trim();
                if (s[i] == "") continue;
                if (obj[s[i]] == null)
                {
                    key = s[i];
                    return false;
                }
            }
            return true;
        }
        public static bool checkApiRunable(string apikey, string apipass)
        {
            var key = "rictotal-dev";
            var pass = "67fa4bc6-cecb-46cb-ae49-8682d6d30beb";

            if (
                String.Compare(apikey, key, false) == 0
                && String.Compare(pass, apipass, false) == 0
                )
                return true;
            return false;
        }

        public static bool IsPhoneNumber(ref string so)
        {
            so = so.Trim();
            so = so.Trim().Replace("+", "").Replace("-", "").Replace(".", "").Replace(" ", "");

            if (so.Length >= 12 || so.Length < 10) return false;

            if (so.StartsWith("0"))
            {
                so = "84" + so.Substring(1);
            }
            return true;
        }
        public static bool IsPhoneNumberValid(string so, ref string tel)
        {
            so = so.Trim().Replace("+", "").Replace("-", "").Replace(".", "").Replace(" ", "");

            if (so.Length >= 12 || so.Length < 10) return false;

            double kq = 0;
            if (!double.TryParse(so, out kq)) return false;
            tel = so;
            return true;
        }
        /// <summary>
        /// send sms api, no statistic (no insert to db)
        /// </summary>
        /// <param name="denSo">numberList, sperate by ;</param>
        /// <param name="noidung"></param>
        /// <param name="msg"></param>
        /// <param name="isonhan"></param>
        /// <returns></returns>
        public static bool SendSMSAPI(string denSo, string noidung, ref string msg)
        {
            string url = "http://210.245.26.64/SMSBrandName/sendSMS.php?secret=df89ej2&brandname=TransTender&mobile=" + denSo + "&content=" + noidung + "";
            System.Net.WebClient client = new System.Net.WebClient();
            string s = client.DownloadString(url);
            return (s == "0" ? true : false);

            //WebRequest request = WebRequest.Create("http://www.google.com");
            //WebResponse response = request.GetResponse();
            //Stream data = response.GetResponseStream();
            //string html = String.Empty;
            //using (StreamReader sr = new StreamReader(data))
            //{
            //    html = sr.ReadToEnd();
            //}
        }

        /// <summary>
        /// send sms and insert to db to statistic
        /// </summary>
        /// <param name="denSo"></param>
        /// <param name="noidung"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static int SendSMS(string denSo, string noidung, ref string msg)
        {
            if (noidung.Trim() == "")
            {
                msg = "No content to send SMS"; return 0;
            }

            if (denSo.Trim() == "")
            {
                msg = "No number list to send SMS"; return 0;
            }

            int dem = 0;
            string[] so = denSo.Split(';');
            string s1;
            string sql;
            for (int i = 0; i < so.Length; i++)
            {
                s1 = so[i];
                if (IsPhoneNumber(ref s1))
                { // gui tin
                    if (SendSMSAPI(s1, noidung, ref msg))
                    {
                        sql = "exec sp_tblSMSSendAuto_Insert N'" + denSo + "',N'" + noidung + "',N'TransTender'";
                        RICDB.DB.RunSQLNoReturn(sql, ref msg, strcnn);
                        dem += 1;

                    }
                }
            }
            return dem;
        }

        public static bool SendEmail(string emailTo, string subject, string noidung, ref string msg)
        {
            string sql;
            sql = "exec sp_tblEmailSendAuto_Insert '',N'" + subject + "',N'" + noidung + "',N'" + emailTo + "',N'TransTender'";
            RICDB.DB.RunProcNoReturn(sql, ref msg, strcnn);
            return (msg == "100" ? true : false);

        }


        public static bool SendEmailDirect(string emailTo, string subject, string noidung, ref string msg)
        {
            msg = "100";
            try
            { 
                SmtpClient SmtpServer = new SmtpClient();
                string email = GetAppSetting("mail_email");

                SmtpServer.Credentials = new System.Net.NetworkCredential(email, GetAppSetting("mail_pwd"));
                SmtpServer.Port =  int.Parse( GetAppSetting("mail_port"));
                SmtpServer.Host = GetAppSetting("mail_server");
                SmtpServer.EnableSsl = true;
                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(email, "CPA VietNam", System.Text.Encoding.UTF8);
                mail.To.Add(emailTo);
                mail.Subject = subject;
                mail.Body = noidung;
                mail.IsBodyHtml = true;
                mail.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;
                SmtpServer.Send(mail);
                SmtpServer.Dispose();
            }
            catch (Exception e)
            {
                msg = e.Message;
                return false;
            }
            return true;
        }


        public static string ActiveMember(string mbid, string code)
        {

            string msg = "", sql;
            sql = "exec sp_tblMembers_Active '" + mbid + "','" + code + "'";

            string strcnn = common.GetAppSetting("cnn");
            RICDB.DB.RunSQLNoReturn(sql, ref msg, strcnn);
            return msg;
        }

        public static string DeleteMember(string mbid, string phone)
        {

            string msg = "", sql;
            sql = "exec [sp_tblMembers_Delete1] '" + mbid + "','" + phone + "'";

            string strcnn = common.GetAppSetting("cnn");
            RICDB.DB.RunSQLNoReturn(sql, ref msg, strcnn);
            return msg;
        }

        public static string GetActiveCode(string mbid, ref string msg)
        {

            string sql;
            sql = "exec [sp_tblMembers_GetActiveCode] '" + mbid + "'";

            string strcnn = common.GetAppSetting("cnn");
            sql = RICDB.DB.RunSQLReturn1Value(sql, ref msg, strcnn);
            return sql;
        }

        public static jsonClientPostUser GetMemberIdByPhoneOREmail(string value, ref string msg)
        {
            jsonClientPostUser clientData = new jsonClientPostUser();
            DataTable dt;
            string sql;
            sql = "exec [sp_tblMembers_Get_ByEmailPhone] '" + value + "'";

            string strcnn = common.GetAppSetting("cnn");
            dt = RICDB.DB.RunSQL(sql, ref msg, strcnn);

            if (msg == "100")
                if (dt.Rows.Count > 0)
                {

                    clientData.userId = dt.Rows[0]["userId"].ToString();
                    clientData.userEmail = dt.Rows[0]["userEmail"].ToString();
                    clientData.userFullName = dt.Rows[0]["userFullName"].ToString();
                    clientData.userTel = dt.Rows[0]["userTel"].ToString();
                    clientData.userUid = dt.Rows[0]["userUid"].ToString();
                    clientData.token = "";
                    clientData.userAvata = dt.Rows[0]["userAvata"].ToString();
                    clientData.langId = dt.Rows[0]["MB_LG_ID"].ToString(); ;
                    try
                    {
                        clientData.userMultiLogin = int.Parse(dt.Rows[0]["userMultiLogin"].ToString());
                    }
                    catch  
                    {
                        clientData.userMultiLogin = 1;

                    }
                    dt.Dispose();
                }
                else
                    msg = "110";
            return clientData;
        }


        //public static string GetMenuAdmin(string userId)
        //{
        //    // may menu -- fill ra dang html
        //    string sql, msg = "";
        //    System.Data.DataTable dt;
        //    string strcnn = System.Configuration.ConfigurationManager.AppSettings["cnn"].ToString();

        //    string mnu = "";
        //    sql = "exec [sp_get_menu_list_4admin] '" + userId + "'";
        //    dt = RICDB.DB.RunSQL(sql, ref msg, strcnn);
        //    if (msg != "100") return "";
        //    if (dt.Rows.Count == 0) return "";

        //    int i;

        //    string muc = dt.Rows[0]["mn_muc"].ToString();
        //    int dau = 0;
        //    int j = 0;
        //    for (i = 0; i < dt.Rows.Count; i++)
        //    {
        //        // truong hop la parent 
        //        if (dt.Rows[i]["mn_muc"].ToString() == "0")
        //        {
        //            if (dau != 0)
        //            {
        //                dau = 1;
        //                mnu += "</li>";
        //            }
        //            mnu += "<li class='treeview'>";
        //            mnu += "          <a href = '" + dt.Rows[i]["mn_url"].ToString() + "' >";
        //            mnu += "              <i class='" + dt.Rows[i]["mn_icon"].ToString() + "'></i> <span> " + dt.Rows[i]["mn_ten"].ToString() + "</span>";
        //            mnu += "              <span class='pull-right-container'>";
        //            mnu += "                  <i class='fa fa-angle-left pull-right'></i>";
        //            mnu += "              </span>";
        //            mnu += "          </a>";

        //        }
        //        else // la con - muc1
        //        {
        //            // duyet cac con cua 1 muc
        //            mnu += "  <ul class='treeview-menu'>"; // bat dau menu con
        //            for (j = i; j < dt.Rows.Count; j++)
        //            {
        //                if (dt.Rows[j]["mn_muc"].ToString() == "0") break;
        //                // submenu item
        //                mnu += "             <li><a href = '" + dt.Rows[j]["mn_url"].ToString() + "' ><i class='" + dt.Rows[j]["mn_icon"].ToString() + "'></i> " + dt.Rows[j]["mn_ten"].ToString() + "</a></li>";
        //            }
        //            mnu += "  </ul>"; // cua sub menu
        //            i = j - 1; // de chay tiep
        //        }

        //    }
        //    if (mnu != "") mnu += "</li>"; // end menu cha

        //    return mnu;
        //}


        // khi quen 

            /// <summary>
            /// gui mat khau cho 1 email
            /// </summary>
            /// <param name="mbid"></param>
            /// <param name="msg"></param>
            /// <returns></returns>
        public static bool emailMatkhau(string email, ref string msg)
        {
            string fs = System.Web.HttpContext.Current.Server.MapPath("~/") + "templatefiles\\forgotpassword.html";

            // doc csdl 
            string sql;
            sql = "exec [sp_tblUser_GetPassword] N'" + email+ "'";
            string strcnn = GetAppSetting("cnn");
            sql = RICDB.DB.RunSQLReturn1Value(sql, ref msg, strcnn);

            if (msg != "100") return false;
            
            string snd;
            try
            {
                snd = File.ReadAllText(fs, System.Text.Encoding.UTF8);
            }
            catch (Exception e)
            {
                msg = e.Message;
                return false;
            }
            snd = snd.Replace("{{UID}}", email).Replace("{{PASSWORD}}", sql.Trim());
            return SendEmailDirect(email, "RICTotal: Lấy lại mật khẩu", snd, ref msg);
        }

        public static bool ImageInsert(string path, string loai, string referenceId, string alt,string rc_id, ref string msg, long size = 0, int w = 0, int h = 0)
        {
            string sql;
            sql = "exec [sp_tblAnh_Insert] N'" + path + "','" + loai +"','" + (referenceId.Trim() ==""?Guid.Empty.ToString():referenceId) + "',N'" + alt +"'," +size.ToString() +"," + w.ToString()  + "," + h.ToString()  + ",'" + rc_id + "'";
            string strcnn = GetAppSetting("cnn");
            sql = RICDB.DB.RunSQLReturn1Value(sql, ref msg, strcnn);
            if (msg != "100") return false;
            sql = "";
            path = path.Replace(System.Web.HttpContext.Current.Server.MapPath("~/"), "");
            path = "/" + path.Replace("\\", "/");
            switch (loai)
            {
                case "logo":
                    sql  = "EXEC [sp_tblRicCustomerList_Update_Logo] '" + rc_id + "','" + path + "'";
                    break;
                case "product":
                   
                    break;
                case "background":
                   
                    break;
                case "category":
                
                    break;
            }

            RICDB.DB.RunSQLNoReturn(sql, ref msg, strcnn);
            if (msg != "100") return false;
            return true;
        }

        public static bool ImageDelete(string id, string path,string rc_id, ref string msg)
        {
            string sql;
            
            sql = "exec [sp_tblAnh_Delete] N'" + id + "','" + path + "','" + rc_id + "'";

            string strcnn = GetAppSetting("cnn");
            sql = RICDB.DB.RunSQLReturn1Value(sql, ref msg, strcnn);

            if (msg != "100") return false;
            return true;
        }


        public static System.Drawing.Bitmap ImageResize(System.Drawing.Image image_file, int max_height, int max_width)
        {
            // string fileName = Server.HtmlEncode(File1.FileName);
            // string extension = System.IO.Path.GetExtension(fileName);
            //System.Drawing.Image image_file = System.Drawing.Image.FromStream(File1.PostedFile.InputStream);
            int image_height = image_file.Height;
            int image_width = image_file.Width;

            image_height = (image_height * max_width) / image_width;
            image_width = max_width;

            if (image_height > max_height)
            {
                image_width = (image_width * max_height) / image_height;
                image_height = max_height;
            }
            System.Drawing.Bitmap bitmap_file = new System.Drawing.Bitmap(image_file, image_width, image_height);
            // bitmap_file.Save(filename, System.Drawing.Imaging.ImageFormat.Png);
            return bitmap_file;
            //  return true;
        }


        public static string ImageSavePath(string type)
        { // lay thu muc luu anh
            string s = "/Uploads/";
            switch (type)
            {
                case "logo":
                    s += "Logos/";
                    break;
                case "product":
                    s += "Products/";
                    break;
                case "background":
                    s += "Backgrounds/";
                    break;
                case "category":
                    s += "Categories/";
                    break;
            }
            return s;
        }

        public static string ImageFileName(string type, string rc_id)
        { // dat ten file tuong ung voi muc dich su dung
            string s = "FL_" + Guid.NewGuid().ToString().Replace("-", "").ToLower() + "_" + ".png";
            switch (type)
            {
                case "logo":
                    s = "LG_" + rc_id.Replace("-", "").ToLower() + ".png";
                    break;
                case "product":
                    s = "P_" + Guid.NewGuid().ToString().Replace("-", "").ToLower() + ".png";
                    break;
                case "background":
                    s = "BG_" + Guid.NewGuid().ToString().Replace("-", "").ToLower() + ".png";
                    break;
                case "category":
                    s = "C_" + Guid.NewGuid().ToString().Replace("-", "").ToLower() + ".png";
                    break;
            }
            return s;
        }
        public static DateTime DMYtoMDY(string s)
        {
            string [] s1 = s.Split('/');
            try
            {
                DateTime d1 = new DateTime(int.Parse(s1[2]), int.Parse(s1[1]), int.Parse(s1[0]));
            }catch  
            {
                return DateTime.Now;
            }
            //YYYY-MM-DDThh:mm:ss.sTZD 
            return new DateTime(int.Parse( s1[2]), int.Parse(s1[1]), int.Parse(s1[0]), DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second, DateTime.Now.Millisecond);
                
        }
        public static string formatDateTime(string format, DateTime dt)
        {
            string s = "";
            s = format.Replace("hh", dt.Hour < 10 ? "0" : "" + dt.Hour.ToString());
                s = s.Replace("mmm", dt.Minute < 10 ? "0" : "" + dt.Minute.ToString());
                s = s.Replace("ss", dt.Second < 10 ? "0" : "" + dt.Second.ToString());
                s = s.Replace("ms", (dt.Second < 10 ? "00" : (dt.Second < 100 ? "0" : "")) + dt.Millisecond.ToString());

                s = s.Replace("YYYY", dt.Year.ToString());
                s = s.Replace("MM", dt.Month < 10 ? "0" : "" + dt.Month.ToString());
                s = s.Replace("DD", dt.Day < 10 ? "0" : "" + dt.Day.ToString());
                
            return s;
        }

        private static readonly string[] VietNamChar = new string[] 
        { 
            "aAeEoOuUiIdDyY", 
            "áàạảãâấầậẩẫăắằặẳẵ", 
            "ÁÀẠẢÃÂẤẦẬẨẪĂẮẰẶẲẴ", 
            "éèẹẻẽêếềệểễ", 
            "ÉÈẸẺẼÊẾỀỆỂỄ", 
            "óòọỏõôốồộổỗơớờợởỡ", 
            "ÓÒỌỎÕÔỐỒỘỔỖƠỚỜỢỞỠ", 
            "úùụủũưứừựửữ", 
            "ÚÙỤỦŨƯỨỪỰỬỮ", 
            "íìịỉĩ", 
            "ÍÌỊỈĨ", 
            "đ", 
            "Đ", 
            "ýỳỵỷỹ", 
            "ÝỲỴỶỸ" 
         };
        public static string LocDau(string str)
        {
            //Thay thế và lọc dấu từng char      
            for (int i = 1; i < VietNamChar.Length; i++)
            {
                for (int j = 0; j < VietNamChar[i].Length; j++)
                    str = str.Replace(VietNamChar[i][j], VietNamChar[0][i - 1]);
            }
            return str;
        }

        public static string _createURLFromText(string text)
        {
            // text = "Ông Nguyễn Phú Hà – Chủ tịch HĐTV - Công ty TNHH Kiểm toán CPA VIETNAM tham dự \"Lễ khai giảng Trường PTDTBT TH & THCS Tả\" Sử Chóong năm học 2019-2020.";
            string url = LocDau(text);
            string myString = Regex.Replace(url, @"[^0-9a-zA-Z]+", " ");
            myString = myString.Trim();
            myString = myString.Replace(" ", "-");
            myString = Regex.Replace(myString, @"[[-]{2}", String.Empty);

            return myString;
        }

        public static DataTable GetNewDetail(string id)
        {
            DataTable dt = null;
            string sql = "exec sp_tblContentArticles_Get_Detail  " + id + "," + System.Web.HttpContext.Current.Session["lg_id"].ToString();
            try
            {
                dt = RICDB.DB.RunSQL(sql, ref msg, strcnn);

            }
            catch  
            {

            }

            return dt;
        }



    } // end class
} // end namespace


