using BMSCommon.Model;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace BiblePay.BMS.DSQL
{

    public static class HtmlHelperExtensions
    {
        public static HtmlString Bold(this IHtmlHelper html, string text)
        {
            string s= "<b>" + text + "</b>\n";
			return new HtmlString(s);
        }
    }


    public static class ControllerExtensions2
	{

		public static object ChangeObjectType(object value, Type type)
		{
			if (type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
			{
				if (value == null)
				{
					return null;
				}

				return Convert.ChangeType(value, Nullable.GetUnderlyingType(type));
			}
			string sSourceType = value.GetType().ToString();
			string sDestType = type.ToString();
			if (sDestType == "System.String" && sSourceType == "System.Guid")
			{
				return value.ToString();
			}
			return Convert.ChangeType(value, type);
		}

		public static void BindObject<T>(object item, ClientToServer cts)
		{

			Type t = typeof(T);
			var myFields = item.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
			foreach (var prop in myFields)
			{
				string newValue = DOMItem.GetFormData(cts.FormData, prop.Name);
				if (prop != null && !String.IsNullOrEmpty(newValue))
				{
					prop.SetValue(item, ChangeObjectType(newValue, prop.FieldType));
				}
			}

			var myProps = item.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

			foreach (var prop in myProps)
			{
				string newValue = DOMItem.GetFormData(cts.FormData, prop.Name);
				if (prop != null && !String.IsNullOrEmpty(newValue))
				{
					prop.SetValue(item, ChangeObjectType(newValue, prop.PropertyType), null);
				}

			}

		}


		public static object ChangeObjectTypeForFormData(object value, Type type)
		{
			if (type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
			{
				if (value == null)
				{
					return null;
				}

				return Convert.ChangeType(value, Nullable.GetUnderlyingType(type));
			}
			string sSourceType = value.GetType().ToString();
			string sDestType = type.ToString();
			if (sDestType == "System.String" && sSourceType == "System.Guid")
			{
				return value.ToString();
			}
			return Convert.ChangeType(value, type);
		}

		public static void BindObjectToForm<T>(object item, ClientToServer cts)
		{

			Type t = typeof(T);
			var myFields = item.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
			foreach (var prop in myFields)
			{
				string newValue = DOMItem.GetFormData(cts.FormData, prop.Name);
				if (prop != null && !String.IsNullOrEmpty(newValue))
				{
					prop.SetValue(item, ChangeObjectTypeForFormData(newValue, prop.FieldType));
				}
			}

			var myProps = item.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

			foreach (var prop in myProps)
			{
				string newValue = DOMItem.GetFormData(cts.FormData, prop.Name);
				if (prop != null && !String.IsNullOrEmpty(newValue))
				{
					prop.SetValue(item, ChangeObjectTypeForFormData(newValue, prop.PropertyType), null);
				}

			}

		}





        public static async Task<JsonResult> RenderDivToClient<TModel>(this Controller controller, string viewName, TModel model, string sDiv, bool partial = false)
        {
		string sMyData = await RenderViewAsync<TModel>(controller, viewName, model, partial);

            //controller.HttpContext.Session.SetObject("formdata", o);
            ServerToClient returnVal = new ServerToClient();
            returnVal.returnbody = "$('#" + sDiv + "').html(`" + sMyData + "`);";
            returnVal.returntype = "javascript";
            string o1 = JsonConvert.SerializeObject(returnVal);
            return controller.Json(o1);
        }




        public static async Task<string> RenderViewAsync<TModel>(this Controller controller, string viewName, TModel model, bool partial = false)
		{
			if (string.IsNullOrEmpty(viewName))
			{
				viewName = controller.ControllerContext.ActionDescriptor.ActionName;
			}

			controller.ViewData.Model = model;

			using (var writer = new StringWriter())
			{
				IViewEngine viewEngine = controller.HttpContext.RequestServices.GetService(typeof(ICompositeViewEngine)) as ICompositeViewEngine;
				ViewEngineResult viewResult = viewEngine.FindView(controller.ControllerContext, viewName, !partial);

				if (viewResult.Success == false)
				{
					return $"A view with the name {viewName} could not be found";
				}

				ViewContext viewContext = new ViewContext(
					controller.ControllerContext,
					viewResult.View,
					controller.ViewData,
					controller.TempData,
					writer,
					new HtmlHelperOptions()
				);

				await viewResult.View.RenderAsync(viewContext);

				return writer.GetStringBuilder().ToString();
			}
		}
	}


		public static class SessionExtensions
        {

    
            public static void SetObject(this ISession session, string key, object value)
            {
                session.SetString(key, JsonConvert.SerializeObject(value));
            }

            public static T GetObject<T>(this ISession session, string key)
            {
                var value = session.GetString(key);
                return value == null ? default(T) : JsonConvert.DeserializeObject<T>(value);
            }

            public static bool ObjectExists(this ISession session, string key)
            {
                var value = session.GetString(key);
                return (value == null ? false : true);
            }

            public static string GetFormValue(this ISession session, string key)
            {
                string sData = String.Empty;
                ClientToServer cts = session.GetObject<ClientToServer>("formdata");
                if (cts != null)
                {
                     sData = DOMItem.GetFormData(cts.FormData, "txtName");
                }
                return sData;
            }
    }
}
