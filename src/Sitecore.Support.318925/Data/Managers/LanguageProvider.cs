using System;
using System.Globalization;
using Sitecore.Diagnostics;
using Sitecore.Globalization;

namespace Sitecore.Support.Data.Managers
{
  public class LanguageProvider : Sitecore.Data.Managers.LanguageProvider
  {
    public override bool IsValidLanguageName(string name)
    {
      Assert.ArgumentNotNull(name, "name");

      if (this.IsKnownToBeInvalid(name))
      {
        return false;
      }

      if (this.LanguageRegistered(name))
      {
        return true;
      }

      try
      {
        CultureAndRegionInfoBuilder builder = this.GetCultureBuilder(name, assert: false);

        return builder != null;
      }
      catch
      {
        return false;
      }
    }

    public override bool RegisterLanguage(string name)
    {
      Assert.ArgumentNotNullOrEmpty(name, "name");

      if (name[0] == '_')
      {
        return false;
      }

      if (this.LanguageRegistered(name))
      {
        return true;
      }

      try
      {
        CultureAndRegionInfoBuilder builder = GetCultureBuilder(name);

        builder.Register();

        this.MarkLanguageAsRegistered(name);

        Log.Info("Custom language registered: " + name, this);

        return true;
      }
      catch (Exception e)
      {
        Log.Error("Attempt to register language failed. Language: " + name, e, this);
        Log.Error("A custom language name must be on the form: isoLanguageCode-isoRegionCode-customName. The language codes are two-letter ISO 639-1, and the regions codes are are two-letter ISO 3166. Also, customName must not exceed 8 characters in length. Valid example: en-US-East. For the full list of requirements, see: http://msdn2.microsoft.com/en-US/library/system.globalization.cultureandregioninfobuilder.cultureandregioninfobuilder.aspx", this);
      }

      Log.Info("Attempting temporary registration. This might work, but with worse performance than a real registration.", this);

      try
      {
        CultureInfo cultureInfo = Language.CreateCultureInfo(name);

        this.MarkLanguageAsRegistered(name);

        Log.Info("Temporary registration succeeded. Culture: " + cultureInfo.Name, this);

        return true;
      }
      catch
      {
        // silent
      }

      return false;
    }

    private CultureAndRegionInfoBuilder GetCultureBuilder(string languageName, bool assert = true)
    {
      string[] parts = StringUtil.Divide(languageName, '-', true);

      if (assert)
      {
        Assert.IsTrue(
          parts.Length >= 2,
          "The custom language name '{0}' is invalid. A custom language name must be on the form: isoLanguageCode-isoRegionCode-customName. The language codes are two-letter ISO 639-1, and the regions codes are are two-letter ISO 3166. Also, customName must not exceed 8 characters in length. Valid example: en-US-East. For the full list of requirements, see: http://msdn2.microsoft.com/en-US/library/system.globalization.cultureandregioninfobuilder.cultureandregioninfobuilder.aspx",
          languageName);
      }

      if (parts.Length < 2)
      {
        return null;
      }

      string cultureName = parts[0].Trim();

      var builder = new CultureAndRegionInfoBuilder(languageName, CultureAndRegionModifiers.None);

      CultureInfo cultureInfo = Language.GetCultureInfo(cultureName);

      if (cultureInfo.NativeName.ToLowerInvariant().Contains("unknown language"))
      {
        return null;
      }

      builder.LoadDataFromCultureInfo(cultureInfo);
      builder.LoadDataFromRegionInfo(new RegionInfo(cultureInfo.LCID));

      return builder;
    }
  }
}