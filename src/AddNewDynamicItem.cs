﻿// Dynamic View Model (lists)
// Copyright (c) 2020 César Roberto de Souza. Licensed under the MIT license
// cesarsouza@gmail.com - http://crsouza.com

using System;
using System.Diagnostics;
using System.Text.Json;

using DynamicVML.Extensions;
using DynamicVML.Internals;

using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc;
using System.Web;

namespace DynamicVML
{
    /// <summary>
    ///   Represents the data object that the client can send to the server to request a new partial view for
    ///   a new list item using ajax. When the controller creates this new partial view, an instance of this 
    ///   class will be stored in the <see cref="ViewDataDictionary">ViewData</see> object of the views under 
    ///   the key <see cref="Constants.NewItemParams"/>. 
    /// </summary>
    /// 
    /// <remarks>
    /// <para>
    ///   An instance of this class should be specified as the single argument of the controller action 
    ///   that should handle the creation of a new item in your application. In addition, your controller 
    ///   action must end by calling one of the 
    ///   <see cref="ControllerExtensions.PartialView(Controller, IDynamicList, AddNewDynamicItem)"/> 
    ///   overloads that will take care of rendering the new item for you. An example can be seen below:
    /// </para>
    /// 
    /// <code language="csharp">
    /// [HttpGet]
    /// public IActionResult AddBook(AddNewDynamicItem parameters)
    /// {
    ///     var newBookViewModel = new BookViewModel()
    ///     {
    ///         Title = "New book",
    ///         PublicationYear = "1994"
    ///     };
    /// 
    ///     return this.PartialView(newBookViewModel, parameters);
    /// }
    /// </code>
    /// >[!NOTE]
    ///   Under normal circumstances, this class should never need to be instantiated directly 
    ///   by your code as it is part of the inner workings of the library.
    /// </remarks>
    /// 
    public class AddNewDynamicItem : Parameters
    {
        /// <summary>
        ///   Gets or sets the id of the div that should receive the new item.
        /// </summary>
        /// 
        public string ContainerId { get; set; } = String.Empty;

        /// <summary>
        ///   Gets any additional view data which may have been passed by the user
        ///   when calling the <see cref="EditorExtensions.ListEditorFor{TModel, TValue}(Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper{TModel}, System.Linq.Expressions.Expression{Func{TModel, TValue}}, string, string, string?, string?, string?, string, object?, string, ListRenderMode, NewItemMethod)">
        ///   Html.EditorFor</see> extension method, represented by a UTF-8 byte 
        ///   array that can be serialized to JSON and posted back to the server.
        /// </summary>
        /// 
        public byte[]? AdditionalViewData { get; set; }

        /// <summary>
        ///   Creates a new instance of <see cref="AddNewDynamicItem"/>.
        /// </summary>
        /// 
        public AddNewDynamicItem(string containerId, string listTemplate, string itemContainerTemplate,
            string itemTemplate, string prefix, ListRenderMode mode, object? additionalViewData)
        {
            this.ContainerId = containerId;
            this.ListTemplate = listTemplate;
            this.ItemContainerTemplate = itemContainerTemplate;
            this.ItemTemplate = itemTemplate;
            this.Prefix = prefix;
            this.Mode = mode;
            if (additionalViewData != null)
                this.AdditionalViewData = JsonSerializer.SerializeToUtf8Bytes(additionalViewData);
        }




        /// <summary>
        ///   Converts this instance to a GET query string representation that can be sent to the server.
        /// </summary>
        /// 
        /// <remarks>
        /// >[!WARNING]
        /// Additional user data is never included in the query string. See <see cref="EditorExtensions.ListEditorFor{TModel, TValue}(Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper{TModel}, System.Linq.Expressions.Expression{Func{TModel, TValue}}, string, string, string?, string?, string?, string, object?, string, ListRenderMode, NewItemMethod)"/>
        /// for more details.
        /// </remarks>
        /// 
        /// <returns>
        ///     An HTTP GET query string containing the key-value pairs of the properties of this class, e.g.:
        ///     <code language="html">
        ///     "?containerId=SgDdaDhJ&amp;prefix=MyForm.MyProperty&amp;ListTemplate=MyTemplate"
        ///     </code>
        /// </returns>
        /// 
        public string ToQueryString()
        {
            if (!DisableTraceWarningsForQueryStringsThatContainAdditionalViewData && AdditionalViewData != null)
            {
                Trace.TraceWarning("DynamicVM: Additional view data cannot be sent over GET, so the data present" +
                    $"in {nameof(AddNewDynamicItem)} will be ignored. If you do not want this behavior, please " +
                    $"add 'method: POST' as an argument to the 'Html.EditorFor()' method.");
                // let's just warn once, otherwise the log will be filled with messages
                DisableTraceWarningsForQueryStringsThatContainAdditionalViewData = true;
            }

            return $"?{nameof(ContainerId)}={ContainerId}"
                + $"&{nameof(ListTemplate)}={HttpUtility.UrlEncode(ListTemplate)}"
                + $"&{nameof(ItemContainerTemplate)}={HttpUtility.UrlEncode(ItemContainerTemplate)}"
                + $"&{nameof(ItemTemplate)}={HttpUtility.UrlEncode(ItemTemplate)}"
                + $"&{nameof(Prefix)}={Prefix}"
                + $"&{nameof(Mode)}={(int)Mode}";
        }

        /// <summary>
        ///   Converts this instance to a JSON representation that can be sent to the server.
        /// </summary>
        /// 
        /// <returns>A JSON containing the key-value pairs of the properties of this class.</returns>
        /// 
        public string ToJSON()
        {
            return JsonSerializer.Serialize(this);
        }

        /// <summary>
        ///   Creates a new instance of the <see cref="AddNewDynamicItem"/> class. This
        ///   is an empty constructor that is necessary to be present in order for
        ///   <see cref="IModelBinder"/> deserialize objects of this class correctly.
        /// </summary>
        /// 
        public AddNewDynamicItem()
        {
            // This empty constructor is required for ModelBinder to work properly
        }



        /// <summary>
        ///   Self explanatory.
        /// </summary>
        /// 
        /// <remarks>
        ///   By default, the library will generate a <see cref="Trace.TraceWarning(string)"/> if you
        ///   specify any <c>additionalViewData</c> to the <see cref="EditorExtensions.ListEditorFor{TModel, TValue}(Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper{TModel}, System.Linq.Expressions.Expression{Func{TModel, TValue}}, string, string, string?, string?, string?, string, object?, string, ListRenderMode, NewItemMethod)"/>
        ///   method while also specifying the HTTP method as <see cref="NewItemMethod.Get"/>. Setting
        ///   this static property to <c>false</c> will disable those warnings globally.
        /// </remarks>
        /// 
        public static bool DisableTraceWarningsForQueryStringsThatContainAdditionalViewData { get; set; } = false;

    }
}
