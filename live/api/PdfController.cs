// Add namespaces to enable security in Oqtane & Dnn despite the differences
#if NETCOREAPP
using Microsoft.AspNetCore.Authorization; // .net core [AllowAnonymous] & [Authorize]
using Microsoft.AspNetCore.Mvc;           // .net core [HttpGet] / [HttpPost] etc.
#else
// 2sxclint:disable:no-dnn-namespaces 2sxclint:disable:no-web-namespaces
using System.Web.Http;
#endif
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Globalization;
using System.Web;

[AllowAnonymous]			// define that all commands can be accessed without a login
public class PdfController : Custom.Hybrid.Api14
{
  // Token is verified in pdf pages to prevent unwanted user access
  public static string generatorUrlPrefix = "/pdf/generator";

  // Print any page url
  [HttpGet]
  public async Task<dynamic> Page(string url)
  {
    if (url == null) {
      return "";
    }

    var pdfService = CreateInstance("../Services/PdfService.cs");
    byte[] pdfStream = await pdfService.GetWebPageAsPdf(url);

    HttpContext.Current.Response.ContentType = "application/pdf";
    HttpContext.Current.Response.AddHeader("Content-Disposition", "filename=" + url + ".pdf");
    HttpContext.Current.Response.BinaryWrite(pdfStream);
    HttpContext.Current.Response.End();

    return pdfStream;
  }

  // Print the document content 
  [HttpGet]
  public async Task<dynamic> Document(int documentId)
  {
    // Check authorization here if data integrity is required

    // Modify print url if your page uses multiple cultures: CmsContext.Site.Url + "/" + cultureCode + ...
    var cultureCode = CmsContext.Culture.CurrentCode.Substring(0, 2);

    var pdfService = CreateInstance("../Services/PdfService.cs");
    var printUrl = pdfService.AddPrintToken(CmsContext.Site.Url + generatorUrlPrefix + "/document?" + "id=" + documentId);

    byte[] pdfStream = await pdfService.GetWebPageAsPdf(printUrl);
    var documentService = CreateInstance("../Services/DocumentService.cs");
    var documentName = documentService.GetDocumentName(documentId);

    HttpContext.Current.Response.ContentType = "application/pdf";
    HttpContext.Current.Response.AddHeader("Content-Disposition", "filename=" + documentName + ".pdf");
    HttpContext.Current.Response.BinaryWrite(pdfStream);
    HttpContext.Current.Response.End();

    return pdfStream;
  }


  // Example: You can also use the document as a template and automatically replace tokens with values
  [HttpGet]
  public async Task<dynamic> TokenDataDocument(int tokenDataId)
  {
    // Check authorization here if data integrity is required

    // Modify print url if your page uses multiple cultures: CmsContext.Site.Url + "/" + cultureCode + ...
    var cultureCode = CmsContext.Culture.CurrentCode.Substring(0, 2);
    var pdfService = CreateInstance("../Services/PdfService.cs");
    var printUrl = pdfService.AddPrintToken(CmsContext.Site.Url + generatorUrlPrefix + "/token-data?" + "id=" + tokenDataId);

    byte[] pdfStream = await pdfService.GetWebPageAsPdf(printUrl);
    var tokenDataDocumentService = CreateInstance("../Services/TokenDataDocumentService.cs");
    var documentName = tokenDataDocumentService.GetTokenDataDocumentName(tokenDataId);

    HttpContext.Current.Response.ContentType = "application/pdf";
    HttpContext.Current.Response.AddHeader("Content-Disposition", "filename=" + documentName + ".pdf");
    HttpContext.Current.Response.BinaryWrite(pdfStream);
    HttpContext.Current.Response.End();

    return pdfStream;
  }

  // Print the tutorial page 
  [HttpGet]
  public async Task<dynamic> TutorialPage()
  {
    // Modify print url if your page uses multiple cultures: CmsContext.Site.Url + "/" + cultureCode + ...
    var cultureCode = CmsContext.Culture.CurrentCode.Substring(0, 2);
    var pdfService = CreateInstance("../Services/PdfService.cs");
    var printUrl = pdfService.AddPrintToken(CmsContext.Site.Url + generatorUrlPrefix + "/tutorial");

    byte[] pdfStream = await pdfService.GetWebPageAsPdf(printUrl);
    var documentName = "Tutorial-Page";

    HttpContext.Current.Response.ContentType = "application/pdf";
    HttpContext.Current.Response.AddHeader("Content-Disposition", "filename=" + documentName + ".pdf");
    HttpContext.Current.Response.BinaryWrite(pdfStream);
    HttpContext.Current.Response.End();

    return pdfStream;
  }
}
