using System;
using System.Globalization;
using System.Linq;

public class DocumentService : Custom.Hybrid.Code14
{
  public string GetDocumentName(int documentId)
  {
    var document = AsList(App.Data["Document"]).Where(doc => doc.EntityId == documentId).FirstOrDefault();

    if (document == null) {
      return DateTime.Now.ToString("/dd/MM/yyyy");
    }

    return AsDynamic(document as object).Name;
  }
}

