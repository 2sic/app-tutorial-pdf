using System;
using PuppeteerSharp;
using PuppeteerSharp.Media;

public class VerifyPuppeteer : Custom.Hybrid.Code14
{
  public bool HasBinary()
  {
    // Will throw exception, because of PuppeteerSharp namespace if binary not added
    return true;
  }
}