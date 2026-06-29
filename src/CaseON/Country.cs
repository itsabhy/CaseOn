using System.Collections.Frozen;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace CaseON;

/// <summary>
/// Reference data and lookups for countries: calling codes, ISO codes, currencies and postal-code
/// patterns. Call as <c>CaseON.Country.GetInfo(CountryCode.India)</c> or, after <c>using CaseON;</c>,
/// simply <c>Country.GetInfo(CountryCode.India)</c>.
/// </summary>
/// <remarks>
/// Data covers the ISO 3166-1 country list. Calling codes follow ITU-T E.164. Postal-code patterns are
/// provided for countries with well-defined formats; where no pattern is available the value is
/// <see langword="null"/> and <see cref="Validate.PostalCode(string?, CountryCode)"/> falls back to a
/// permissive check.
/// </remarks>
public static class Country
{
    private static readonly FrozenDictionary<CountryCode, CountryInfo> ByCode;
    private static readonly FrozenDictionary<string, CountryCode> ByAlpha2;
    private static readonly FrozenDictionary<string, CountryCode> ByAlpha3;

    static Country()
    {
        var data = BuildData();
        ByCode = data.ToFrozenDictionary(static c => c.Code);
        ByAlpha2 = data.ToFrozenDictionary(static c => c.Alpha2, static c => c.Code, StringComparer.OrdinalIgnoreCase);
        ByAlpha3 = data.ToFrozenDictionary(static c => c.Alpha3, static c => c.Code, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>Gets reference data for the specified <paramref name="country"/>.</summary>
    /// <param name="country">The country to look up.</param>
    /// <returns>The <see cref="CountryInfo"/> for the country.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="country"/> is not a known country.</exception>
    public static CountryInfo GetInfo(CountryCode country) =>
        ByCode.TryGetValue(country, out var info)
            ? info
            : throw new ArgumentOutOfRangeException(nameof(country), country, "Unknown country.");

    /// <summary>Gets the ITU-T E.164 international calling code for the specified <paramref name="country"/>.</summary>
    /// <param name="country">The country to look up.</param>
    /// <returns>The numeric calling code (e.g. <c>1</c> for the United States, <c>91</c> for India).</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="country"/> is not a known country.</exception>
    public static int GetCallingCode(CountryCode country) => GetInfo(country).CallingCode;

    /// <summary>Gets the English short name for the specified <paramref name="country"/>.</summary>
    /// <param name="country">The country to look up.</param>
    /// <returns>The country's English name.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="country"/> is not a known country.</exception>
    public static string GetName(CountryCode country) => GetInfo(country).Name;

    /// <summary>Returns reference data for every known country.</summary>
    /// <returns>A read-only collection of <see cref="CountryInfo"/> ordered by name.</returns>
    public static IReadOnlyCollection<CountryInfo> All => ByCode.Values;

    /// <summary>Attempts to resolve a <see cref="CountryCode"/> from an ISO 3166-1 alpha-2 or alpha-3 code.</summary>
    /// <param name="isoCode">The two- or three-letter ISO code (case-insensitive), e.g. <c>US</c> or <c>USA</c>.</param>
    /// <param name="country">When this method returns, contains the matched country if successful.</param>
    /// <returns><see langword="true"/> if a country was resolved; otherwise <see langword="false"/>.</returns>
    public static bool TryParse([NotNullWhen(true)] string? isoCode, out CountryCode country)
    {
        country = default;
        if (string.IsNullOrWhiteSpace(isoCode))
        {
            return false;
        }

        var trimmed = isoCode.Trim();
        return trimmed.Length switch
        {
            2 => ByAlpha2.TryGetValue(trimmed, out country),
            3 => ByAlpha3.TryGetValue(trimmed, out country),
            _ => false,
        };
    }

    private static CountryInfo[] BuildData() =>
    [
        new(CountryCode.Afghanistan, "Afghanistan", "AF", "AFG", 93, "AFN", null),
        new(CountryCode.AlandIslands, "Åland Islands", "AX", "ALA", 358, "EUR", @"^\d{5}$"),
        new(CountryCode.Albania, "Albania", "AL", "ALB", 355, "ALL", @"^\d{4}$"),
        new(CountryCode.Algeria, "Algeria", "DZ", "DZA", 213, "DZD", @"^\d{5}$"),
        new(CountryCode.AmericanSamoa, "American Samoa", "AS", "ASM", 1, "USD", @"^\d{5}(-\d{4})?$"),
        new(CountryCode.Andorra, "Andorra", "AD", "AND", 376, "EUR", @"^AD\d{3}$"),
        new(CountryCode.Angola, "Angola", "AO", "AGO", 244, "AOA", null),
        new(CountryCode.Anguilla, "Anguilla", "AI", "AIA", 1, "XCD", @"^AI-2640$"),
        new(CountryCode.Antarctica, "Antarctica", "AQ", "ATA", 672, null, null),
        new(CountryCode.AntiguaAndBarbuda, "Antigua and Barbuda", "AG", "ATG", 1, "XCD", null),
        new(CountryCode.Argentina, "Argentina", "AR", "ARG", 54, "ARS", @"^([A-Z]\d{4}[A-Z]{3}|\d{4})$"),
        new(CountryCode.Armenia, "Armenia", "AM", "ARM", 374, "AMD", @"^\d{4}$"),
        new(CountryCode.Aruba, "Aruba", "AW", "ABW", 297, "AWG", null),
        new(CountryCode.Australia, "Australia", "AU", "AUS", 61, "AUD", @"^\d{4}$"),
        new(CountryCode.Austria, "Austria", "AT", "AUT", 43, "EUR", @"^\d{4}$"),
        new(CountryCode.Azerbaijan, "Azerbaijan", "AZ", "AZE", 994, "AZN", @"^AZ ?\d{4}$"),
        new(CountryCode.Bahamas, "Bahamas", "BS", "BHS", 1, "BSD", null),
        new(CountryCode.Bahrain, "Bahrain", "BH", "BHR", 973, "BHD", @"^\d{3,4}$"),
        new(CountryCode.Bangladesh, "Bangladesh", "BD", "BGD", 880, "BDT", @"^\d{4}$"),
        new(CountryCode.Barbados, "Barbados", "BB", "BRB", 1, "BBD", @"^BB\d{5}$"),
        new(CountryCode.Belarus, "Belarus", "BY", "BLR", 375, "BYN", @"^\d{6}$"),
        new(CountryCode.Belgium, "Belgium", "BE", "BEL", 32, "EUR", @"^\d{4}$"),
        new(CountryCode.Belize, "Belize", "BZ", "BLZ", 501, "BZD", null),
        new(CountryCode.Benin, "Benin", "BJ", "BEN", 229, "XOF", null),
        new(CountryCode.Bermuda, "Bermuda", "BM", "BMU", 1, "BMD", @"^[A-Z]{2} ?[A-Z0-9]{2}$"),
        new(CountryCode.Bhutan, "Bhutan", "BT", "BTN", 975, "BTN", @"^\d{5}$"),
        new(CountryCode.Bolivia, "Bolivia", "BO", "BOL", 591, "BOB", null),
        new(CountryCode.BonaireSintEustatiusAndSaba, "Bonaire, Sint Eustatius and Saba", "BQ", "BES", 599, "USD", null),
        new(CountryCode.BosniaAndHerzegovina, "Bosnia and Herzegovina", "BA", "BIH", 387, "BAM", @"^\d{5}$"),
        new(CountryCode.Botswana, "Botswana", "BW", "BWA", 267, "BWP", null),
        new(CountryCode.BouvetIsland, "Bouvet Island", "BV", "BVT", 47, "NOK", null),
        new(CountryCode.Brazil, "Brazil", "BR", "BRA", 55, "BRL", @"^\d{5}-?\d{3}$"),
        new(CountryCode.BritishIndianOceanTerritory, "British Indian Ocean Territory", "IO", "IOT", 246, "USD", @"^BBND 1ZZ$"),
        new(CountryCode.Brunei, "Brunei Darussalam", "BN", "BRN", 673, "BND", @"^[A-Z]{2}\d{4}$"),
        new(CountryCode.Bulgaria, "Bulgaria", "BG", "BGR", 359, "BGN", @"^\d{4}$"),
        new(CountryCode.BurkinaFaso, "Burkina Faso", "BF", "BFA", 226, "XOF", null),
        new(CountryCode.Burundi, "Burundi", "BI", "BDI", 257, "BIF", null),
        new(CountryCode.CaboVerde, "Cabo Verde", "CV", "CPV", 238, "CVE", @"^\d{4}$"),
        new(CountryCode.Cambodia, "Cambodia", "KH", "KHM", 855, "KHR", @"^\d{5,6}$"),
        new(CountryCode.Cameroon, "Cameroon", "CM", "CMR", 237, "XAF", null),
        new(CountryCode.Canada, "Canada", "CA", "CAN", 1, "CAD", @"^[A-Za-z]\d[A-Za-z] ?\d[A-Za-z]\d$"),
        new(CountryCode.CaymanIslands, "Cayman Islands", "KY", "CYM", 1, "KYD", @"^[A-Z]{2}\d-\d{4}$"),
        new(CountryCode.CentralAfricanRepublic, "Central African Republic", "CF", "CAF", 236, "XAF", null),
        new(CountryCode.Chad, "Chad", "TD", "TCD", 235, "XAF", null),
        new(CountryCode.Chile, "Chile", "CL", "CHL", 56, "CLP", @"^\d{7}$"),
        new(CountryCode.China, "China", "CN", "CHN", 86, "CNY", @"^\d{6}$"),
        new(CountryCode.ChristmasIsland, "Christmas Island", "CX", "CXR", 61, "AUD", @"^\d{4}$"),
        new(CountryCode.CocosIslands, "Cocos (Keeling) Islands", "CC", "CCK", 61, "AUD", @"^\d{4}$"),
        new(CountryCode.Colombia, "Colombia", "CO", "COL", 57, "COP", @"^\d{6}$"),
        new(CountryCode.Comoros, "Comoros", "KM", "COM", 269, "KMF", null),
        new(CountryCode.Congo, "Congo", "CG", "COG", 242, "XAF", null),
        new(CountryCode.CongoDemocraticRepublic, "Congo, Democratic Republic of the", "CD", "COD", 243, "CDF", null),
        new(CountryCode.CookIslands, "Cook Islands", "CK", "COK", 682, "NZD", null),
        new(CountryCode.CostaRica, "Costa Rica", "CR", "CRI", 506, "CRC", @"^\d{5}$"),
        new(CountryCode.CoteDIvoire, "Côte d'Ivoire", "CI", "CIV", 225, "XOF", null),
        new(CountryCode.Croatia, "Croatia", "HR", "HRV", 385, "EUR", @"^\d{5}$"),
        new(CountryCode.Cuba, "Cuba", "CU", "CUB", 53, "CUP", @"^\d{5}$"),
        new(CountryCode.Curacao, "Curaçao", "CW", "CUW", 599, "ANG", null),
        new(CountryCode.Cyprus, "Cyprus", "CY", "CYP", 357, "EUR", @"^\d{4}$"),
        new(CountryCode.Czechia, "Czechia", "CZ", "CZE", 420, "CZK", @"^\d{3} ?\d{2}$"),
        new(CountryCode.Denmark, "Denmark", "DK", "DNK", 45, "DKK", @"^\d{4}$"),
        new(CountryCode.Djibouti, "Djibouti", "DJ", "DJI", 253, "DJF", null),
        new(CountryCode.Dominica, "Dominica", "DM", "DMA", 1, "XCD", null),
        new(CountryCode.DominicanRepublic, "Dominican Republic", "DO", "DOM", 1, "DOP", @"^\d{5}$"),
        new(CountryCode.Ecuador, "Ecuador", "EC", "ECU", 593, "USD", @"^\d{6}$"),
        new(CountryCode.Egypt, "Egypt", "EG", "EGY", 20, "EGP", @"^\d{5}$"),
        new(CountryCode.ElSalvador, "El Salvador", "SV", "SLV", 503, "USD", @"^\d{4}$"),
        new(CountryCode.EquatorialGuinea, "Equatorial Guinea", "GQ", "GNQ", 240, "XAF", null),
        new(CountryCode.Eritrea, "Eritrea", "ER", "ERI", 291, "ERN", null),
        new(CountryCode.Estonia, "Estonia", "EE", "EST", 372, "EUR", @"^\d{5}$"),
        new(CountryCode.Eswatini, "Eswatini", "SZ", "SWZ", 268, "SZL", @"^[A-Z]\d{3}$"),
        new(CountryCode.Ethiopia, "Ethiopia", "ET", "ETH", 251, "ETB", @"^\d{4}$"),
        new(CountryCode.FalklandIslands, "Falkland Islands", "FK", "FLK", 500, "FKP", @"^FIQQ 1ZZ$"),
        new(CountryCode.FaroeIslands, "Faroe Islands", "FO", "FRO", 298, "DKK", @"^\d{3}$"),
        new(CountryCode.Fiji, "Fiji", "FJ", "FJI", 679, "FJD", null),
        new(CountryCode.Finland, "Finland", "FI", "FIN", 358, "EUR", @"^\d{5}$"),
        new(CountryCode.France, "France", "FR", "FRA", 33, "EUR", @"^\d{5}$"),
        new(CountryCode.FrenchGuiana, "French Guiana", "GF", "GUF", 594, "EUR", @"^973\d{2}$"),
        new(CountryCode.FrenchPolynesia, "French Polynesia", "PF", "PYF", 689, "XPF", @"^987\d{2}$"),
        new(CountryCode.FrenchSouthernTerritories, "French Southern Territories", "TF", "ATF", 262, "EUR", null),
        new(CountryCode.Gabon, "Gabon", "GA", "GAB", 241, "XAF", null),
        new(CountryCode.Gambia, "Gambia", "GM", "GMB", 220, "GMD", null),
        new(CountryCode.Georgia, "Georgia", "GE", "GEO", 995, "GEL", @"^\d{4}$"),
        new(CountryCode.Germany, "Germany", "DE", "DEU", 49, "EUR", @"^\d{5}$"),
        new(CountryCode.Ghana, "Ghana", "GH", "GHA", 233, "GHS", null),
        new(CountryCode.Gibraltar, "Gibraltar", "GI", "GIB", 350, "GIP", @"^GX11 1AA$"),
        new(CountryCode.Greece, "Greece", "GR", "GRC", 30, "EUR", @"^\d{3} ?\d{2}$"),
        new(CountryCode.Greenland, "Greenland", "GL", "GRL", 299, "DKK", @"^\d{4}$"),
        new(CountryCode.Grenada, "Grenada", "GD", "GRD", 1, "XCD", null),
        new(CountryCode.Guadeloupe, "Guadeloupe", "GP", "GLP", 590, "EUR", @"^971\d{2}$"),
        new(CountryCode.Guam, "Guam", "GU", "GUM", 1, "USD", @"^\d{5}(-\d{4})?$"),
        new(CountryCode.Guatemala, "Guatemala", "GT", "GTM", 502, "GTQ", @"^\d{5}$"),
        new(CountryCode.Guernsey, "Guernsey", "GG", "GGY", 44, "GBP", @"^GY\d ?\d[A-Z]{2}$"),
        new(CountryCode.Guinea, "Guinea", "GN", "GIN", 224, "GNF", null),
        new(CountryCode.GuineaBissau, "Guinea-Bissau", "GW", "GNB", 245, "XOF", @"^\d{4}$"),
        new(CountryCode.Guyana, "Guyana", "GY", "GUY", 592, "GYD", null),
        new(CountryCode.Haiti, "Haiti", "HT", "HTI", 509, "HTG", @"^HT\d{4}$"),
        new(CountryCode.HeardIslandAndMcDonaldIslands, "Heard Island and McDonald Islands", "HM", "HMD", 672, "AUD", null),
        new(CountryCode.HolySee, "Holy See", "VA", "VAT", 39, "EUR", @"^120$"),
        new(CountryCode.Honduras, "Honduras", "HN", "HND", 504, "HNL", @"^\d{5}$"),
        new(CountryCode.HongKong, "Hong Kong", "HK", "HKG", 852, "HKD", null),
        new(CountryCode.Hungary, "Hungary", "HU", "HUN", 36, "HUF", @"^\d{4}$"),
        new(CountryCode.Iceland, "Iceland", "IS", "ISL", 354, "ISK", @"^\d{3}$"),
        new(CountryCode.India, "India", "IN", "IND", 91, "INR", @"^\d{6}$"),
        new(CountryCode.Indonesia, "Indonesia", "ID", "IDN", 62, "IDR", @"^\d{5}$"),
        new(CountryCode.Iran, "Iran", "IR", "IRN", 98, "IRR", @"^\d{5}-?\d{5}$"),
        new(CountryCode.Iraq, "Iraq", "IQ", "IRQ", 964, "IQD", @"^\d{5}$"),
        new(CountryCode.Ireland, "Ireland", "IE", "IRL", 353, "EUR", null),
        new(CountryCode.IsleOfMan, "Isle of Man", "IM", "IMN", 44, "GBP", @"^IM\d ?\d[A-Z]{2}$"),
        new(CountryCode.Israel, "Israel", "IL", "ISR", 972, "ILS", @"^\d{5}(\d{2})?$"),
        new(CountryCode.Italy, "Italy", "IT", "ITA", 39, "EUR", @"^\d{5}$"),
        new(CountryCode.Jamaica, "Jamaica", "JM", "JAM", 1, "JMD", null),
        new(CountryCode.Japan, "Japan", "JP", "JPN", 81, "JPY", @"^\d{3}-?\d{4}$"),
        new(CountryCode.Jersey, "Jersey", "JE", "JEY", 44, "GBP", @"^JE\d ?\d[A-Z]{2}$"),
        new(CountryCode.Jordan, "Jordan", "JO", "JOR", 962, "JOD", @"^\d{5}$"),
        new(CountryCode.Kazakhstan, "Kazakhstan", "KZ", "KAZ", 7, "KZT", @"^\d{6}$"),
        new(CountryCode.Kenya, "Kenya", "KE", "KEN", 254, "KES", @"^\d{5}$"),
        new(CountryCode.Kiribati, "Kiribati", "KI", "KIR", 686, "AUD", null),
        new(CountryCode.NorthKorea, "North Korea", "KP", "PRK", 850, "KPW", null),
        new(CountryCode.SouthKorea, "South Korea", "KR", "KOR", 82, "KRW", @"^\d{5}$"),
        new(CountryCode.Kuwait, "Kuwait", "KW", "KWT", 965, "KWD", @"^\d{5}$"),
        new(CountryCode.Kyrgyzstan, "Kyrgyzstan", "KG", "KGZ", 996, "KGS", @"^\d{6}$"),
        new(CountryCode.Laos, "Laos", "LA", "LAO", 856, "LAK", @"^\d{5}$"),
        new(CountryCode.Latvia, "Latvia", "LV", "LVA", 371, "EUR", @"^LV-\d{4}$"),
        new(CountryCode.Lebanon, "Lebanon", "LB", "LBN", 961, "LBP", @"^\d{4} ?\d{4}$"),
        new(CountryCode.Lesotho, "Lesotho", "LS", "LSO", 266, "LSL", @"^\d{3}$"),
        new(CountryCode.Liberia, "Liberia", "LR", "LBR", 231, "LRD", @"^\d{4}$"),
        new(CountryCode.Libya, "Libya", "LY", "LBY", 218, "LYD", null),
        new(CountryCode.Liechtenstein, "Liechtenstein", "LI", "LIE", 423, "CHF", @"^\d{4}$"),
        new(CountryCode.Lithuania, "Lithuania", "LT", "LTU", 370, "EUR", @"^LT-\d{5}$"),
        new(CountryCode.Luxembourg, "Luxembourg", "LU", "LUX", 352, "EUR", @"^\d{4}$"),
        new(CountryCode.Macao, "Macao", "MO", "MAC", 853, "MOP", null),
        new(CountryCode.Madagascar, "Madagascar", "MG", "MDG", 261, "MGA", @"^\d{3}$"),
        new(CountryCode.Malawi, "Malawi", "MW", "MWI", 265, "MWK", null),
        new(CountryCode.Malaysia, "Malaysia", "MY", "MYS", 60, "MYR", @"^\d{5}$"),
        new(CountryCode.Maldives, "Maldives", "MV", "MDV", 960, "MVR", @"^\d{5}$"),
        new(CountryCode.Mali, "Mali", "ML", "MLI", 223, "XOF", null),
        new(CountryCode.Malta, "Malta", "MT", "MLT", 356, "EUR", @"^[A-Z]{3} ?\d{4}$"),
        new(CountryCode.MarshallIslands, "Marshall Islands", "MH", "MHL", 692, "USD", @"^\d{5}(-\d{4})?$"),
        new(CountryCode.Martinique, "Martinique", "MQ", "MTQ", 596, "EUR", @"^972\d{2}$"),
        new(CountryCode.Mauritania, "Mauritania", "MR", "MRT", 222, "MRU", null),
        new(CountryCode.Mauritius, "Mauritius", "MU", "MUS", 230, "MUR", @"^\d{5}$"),
        new(CountryCode.Mayotte, "Mayotte", "YT", "MYT", 262, "EUR", @"^976\d{2}$"),
        new(CountryCode.Mexico, "Mexico", "MX", "MEX", 52, "MXN", @"^\d{5}$"),
        new(CountryCode.Micronesia, "Micronesia", "FM", "FSM", 691, "USD", @"^\d{5}(-\d{4})?$"),
        new(CountryCode.Moldova, "Moldova", "MD", "MDA", 373, "MDL", @"^MD-?\d{4}$"),
        new(CountryCode.Monaco, "Monaco", "MC", "MCO", 377, "EUR", @"^980\d{2}$"),
        new(CountryCode.Mongolia, "Mongolia", "MN", "MNG", 976, "MNT", @"^\d{5}$"),
        new(CountryCode.Montenegro, "Montenegro", "ME", "MNE", 382, "EUR", @"^\d{5}$"),
        new(CountryCode.Montserrat, "Montserrat", "MS", "MSR", 1, "XCD", @"^MSR ?\d{4}$"),
        new(CountryCode.Morocco, "Morocco", "MA", "MAR", 212, "MAD", @"^\d{5}$"),
        new(CountryCode.Mozambique, "Mozambique", "MZ", "MOZ", 258, "MZN", @"^\d{4}$"),
        new(CountryCode.Myanmar, "Myanmar", "MM", "MMR", 95, "MMK", @"^\d{5}$"),
        new(CountryCode.Namibia, "Namibia", "NA", "NAM", 264, "NAD", null),
        new(CountryCode.Nauru, "Nauru", "NR", "NRU", 674, "AUD", null),
        new(CountryCode.Nepal, "Nepal", "NP", "NPL", 977, "NPR", @"^\d{5}$"),
        new(CountryCode.Netherlands, "Netherlands", "NL", "NLD", 31, "EUR", @"^\d{4} ?[A-Z]{2}$"),
        new(CountryCode.NewCaledonia, "New Caledonia", "NC", "NCL", 687, "XPF", @"^988\d{2}$"),
        new(CountryCode.NewZealand, "New Zealand", "NZ", "NZL", 64, "NZD", @"^\d{4}$"),
        new(CountryCode.Nicaragua, "Nicaragua", "NI", "NIC", 505, "NIO", @"^\d{5}$"),
        new(CountryCode.Niger, "Niger", "NE", "NER", 227, "XOF", @"^\d{4}$"),
        new(CountryCode.Nigeria, "Nigeria", "NG", "NGA", 234, "NGN", @"^\d{6}$"),
        new(CountryCode.Niue, "Niue", "NU", "NIU", 683, "NZD", null),
        new(CountryCode.NorfolkIsland, "Norfolk Island", "NF", "NFK", 672, "AUD", @"^\d{4}$"),
        new(CountryCode.NorthMacedonia, "North Macedonia", "MK", "MKD", 389, "MKD", @"^\d{4}$"),
        new(CountryCode.NorthernMarianaIslands, "Northern Mariana Islands", "MP", "MNP", 1, "USD", @"^\d{5}(-\d{4})?$"),
        new(CountryCode.Norway, "Norway", "NO", "NOR", 47, "NOK", @"^\d{4}$"),
        new(CountryCode.Oman, "Oman", "OM", "OMN", 968, "OMR", @"^\d{3}$"),
        new(CountryCode.Pakistan, "Pakistan", "PK", "PAK", 92, "PKR", @"^\d{5}$"),
        new(CountryCode.Palau, "Palau", "PW", "PLW", 680, "USD", @"^\d{5}$"),
        new(CountryCode.Palestine, "Palestine", "PS", "PSE", 970, "ILS", null),
        new(CountryCode.Panama, "Panama", "PA", "PAN", 507, "PAB", @"^\d{4}$"),
        new(CountryCode.PapuaNewGuinea, "Papua New Guinea", "PG", "PNG", 675, "PGK", @"^\d{3}$"),
        new(CountryCode.Paraguay, "Paraguay", "PY", "PRY", 595, "PYG", @"^\d{4}$"),
        new(CountryCode.Peru, "Peru", "PE", "PER", 51, "PEN", @"^\d{5}$"),
        new(CountryCode.Philippines, "Philippines", "PH", "PHL", 63, "PHP", @"^\d{4}$"),
        new(CountryCode.Pitcairn, "Pitcairn", "PN", "PCN", 64, "NZD", @"^PCRN 1ZZ$"),
        new(CountryCode.Poland, "Poland", "PL", "POL", 48, "PLN", @"^\d{2}-\d{3}$"),
        new(CountryCode.Portugal, "Portugal", "PT", "PRT", 351, "EUR", @"^\d{4}-\d{3}$"),
        new(CountryCode.PuertoRico, "Puerto Rico", "PR", "PRI", 1, "USD", @"^\d{5}(-\d{4})?$"),
        new(CountryCode.Qatar, "Qatar", "QA", "QAT", 974, "QAR", null),
        new(CountryCode.Reunion, "Réunion", "RE", "REU", 262, "EUR", @"^974\d{2}$"),
        new(CountryCode.Romania, "Romania", "RO", "ROU", 40, "RON", @"^\d{6}$"),
        new(CountryCode.Russia, "Russia", "RU", "RUS", 7, "RUB", @"^\d{6}$"),
        new(CountryCode.Rwanda, "Rwanda", "RW", "RWA", 250, "RWF", null),
        new(CountryCode.SaintBarthelemy, "Saint Barthélemy", "BL", "BLM", 590, "EUR", @"^977\d{2}$"),
        new(CountryCode.SaintHelena, "Saint Helena", "SH", "SHN", 290, "SHP", @"^(STHL|ASCN|TDCU) 1ZZ$"),
        new(CountryCode.SaintKittsAndNevis, "Saint Kitts and Nevis", "KN", "KNA", 1, "XCD", null),
        new(CountryCode.SaintLucia, "Saint Lucia", "LC", "LCA", 1, "XCD", @"^LC\d{2} ?\d{3}$"),
        new(CountryCode.SaintMartin, "Saint Martin", "MF", "MAF", 590, "EUR", @"^978\d{2}$"),
        new(CountryCode.SaintPierreAndMiquelon, "Saint Pierre and Miquelon", "PM", "SPM", 508, "EUR", @"^975\d{2}$"),
        new(CountryCode.SaintVincentAndTheGrenadines, "Saint Vincent and the Grenadines", "VC", "VCT", 1, "XCD", @"^VC\d{4}$"),
        new(CountryCode.Samoa, "Samoa", "WS", "WSM", 685, "WST", @"^WS\d{4}$"),
        new(CountryCode.SanMarino, "San Marino", "SM", "SMR", 378, "EUR", @"^4789\d$"),
        new(CountryCode.SaoTomeAndPrincipe, "Sao Tome and Principe", "ST", "STP", 239, "STN", null),
        new(CountryCode.SaudiArabia, "Saudi Arabia", "SA", "SAU", 966, "SAR", @"^\d{5}(-\d{4})?$"),
        new(CountryCode.Senegal, "Senegal", "SN", "SEN", 221, "XOF", @"^\d{5}$"),
        new(CountryCode.Serbia, "Serbia", "RS", "SRB", 381, "RSD", @"^\d{5,6}$"),
        new(CountryCode.Seychelles, "Seychelles", "SC", "SYC", 248, "SCR", null),
        new(CountryCode.SierraLeone, "Sierra Leone", "SL", "SLE", 232, "SLE", null),
        new(CountryCode.Singapore, "Singapore", "SG", "SGP", 65, "SGD", @"^\d{6}$"),
        new(CountryCode.SintMaarten, "Sint Maarten", "SX", "SXM", 1, "ANG", null),
        new(CountryCode.Slovakia, "Slovakia", "SK", "SVK", 421, "EUR", @"^\d{3} ?\d{2}$"),
        new(CountryCode.Slovenia, "Slovenia", "SI", "SVN", 386, "EUR", @"^\d{4}$"),
        new(CountryCode.SolomonIslands, "Solomon Islands", "SB", "SLB", 677, "SBD", null),
        new(CountryCode.Somalia, "Somalia", "SO", "SOM", 252, "SOS", @"^[A-Z]{2} ?\d{5}$"),
        new(CountryCode.SouthAfrica, "South Africa", "ZA", "ZAF", 27, "ZAR", @"^\d{4}$"),
        new(CountryCode.SouthGeorgia, "South Georgia and the South Sandwich Islands", "GS", "SGS", 500, "GBP", @"^SIQQ 1ZZ$"),
        new(CountryCode.SouthSudan, "South Sudan", "SS", "SSD", 211, "SSP", null),
        new(CountryCode.Spain, "Spain", "ES", "ESP", 34, "EUR", @"^\d{5}$"),
        new(CountryCode.SriLanka, "Sri Lanka", "LK", "LKA", 94, "LKR", @"^\d{5}$"),
        new(CountryCode.Sudan, "Sudan", "SD", "SDN", 249, "SDG", @"^\d{5}$"),
        new(CountryCode.Suriname, "Suriname", "SR", "SUR", 597, "SRD", null),
        new(CountryCode.SvalbardAndJanMayen, "Svalbard and Jan Mayen", "SJ", "SJM", 47, "NOK", @"^\d{4}$"),
        new(CountryCode.Sweden, "Sweden", "SE", "SWE", 46, "SEK", @"^\d{3} ?\d{2}$"),
        new(CountryCode.Switzerland, "Switzerland", "CH", "CHE", 41, "CHF", @"^\d{4}$"),
        new(CountryCode.Syria, "Syria", "SY", "SYR", 963, "SYP", null),
        new(CountryCode.Taiwan, "Taiwan", "TW", "TWN", 886, "TWD", @"^\d{3}(\d{2})?$"),
        new(CountryCode.Tajikistan, "Tajikistan", "TJ", "TJK", 992, "TJS", @"^\d{6}$"),
        new(CountryCode.Tanzania, "Tanzania", "TZ", "TZA", 255, "TZS", @"^\d{5}$"),
        new(CountryCode.Thailand, "Thailand", "TH", "THA", 66, "THB", @"^\d{5}$"),
        new(CountryCode.TimorLeste, "Timor-Leste", "TL", "TLS", 670, "USD", null),
        new(CountryCode.Togo, "Togo", "TG", "TGO", 228, "XOF", null),
        new(CountryCode.Tokelau, "Tokelau", "TK", "TKL", 690, "NZD", null),
        new(CountryCode.Tonga, "Tonga", "TO", "TON", 676, "TOP", null),
        new(CountryCode.TrinidadAndTobago, "Trinidad and Tobago", "TT", "TTO", 1, "TTD", @"^\d{6}$"),
        new(CountryCode.Tunisia, "Tunisia", "TN", "TUN", 216, "TND", @"^\d{4}$"),
        new(CountryCode.Turkey, "Türkiye", "TR", "TUR", 90, "TRY", @"^\d{5}$"),
        new(CountryCode.Turkmenistan, "Turkmenistan", "TM", "TKM", 993, "TMT", @"^\d{6}$"),
        new(CountryCode.TurksAndCaicosIslands, "Turks and Caicos Islands", "TC", "TCA", 1, "USD", @"^TKCA 1ZZ$"),
        new(CountryCode.Tuvalu, "Tuvalu", "TV", "TUV", 688, "AUD", null),
        new(CountryCode.Uganda, "Uganda", "UG", "UGA", 256, "UGX", null),
        new(CountryCode.Ukraine, "Ukraine", "UA", "UKR", 380, "UAH", @"^\d{5}$"),
        new(CountryCode.UnitedArabEmirates, "United Arab Emirates", "AE", "ARE", 971, "AED", null),
        new(CountryCode.UnitedKingdom, "United Kingdom", "GB", "GBR", 44, "GBP", @"^(?:[A-Za-z0-9]{1,2}\d[A-Za-z0-9]? ?\d[A-Za-z]{2})$"),
        new(CountryCode.UnitedStates, "United States", "US", "USA", 1, "USD", @"^\d{5}(-\d{4})?$"),
        new(CountryCode.UnitedStatesMinorOutlyingIslands, "United States Minor Outlying Islands", "UM", "UMI", 1, "USD", @"^\d{5}(-\d{4})?$"),
        new(CountryCode.Uruguay, "Uruguay", "UY", "URY", 598, "UYU", @"^\d{5}$"),
        new(CountryCode.Uzbekistan, "Uzbekistan", "UZ", "UZB", 998, "UZS", @"^\d{6}$"),
        new(CountryCode.Vanuatu, "Vanuatu", "VU", "VUT", 678, "VUV", null),
        new(CountryCode.Venezuela, "Venezuela", "VE", "VEN", 58, "VES", @"^\d{4}(-[A-Z])?$"),
        new(CountryCode.Vietnam, "Vietnam", "VN", "VNM", 84, "VND", @"^\d{6}$"),
        new(CountryCode.BritishVirginIslands, "British Virgin Islands", "VG", "VGB", 1, "USD", @"^VG\d{4}$"),
        new(CountryCode.UsVirginIslands, "U.S. Virgin Islands", "VI", "VIR", 1, "USD", @"^\d{5}(-\d{4})?$"),
        new(CountryCode.WallisAndFutuna, "Wallis and Futuna", "WF", "WLF", 681, "XPF", @"^986\d{2}$"),
        new(CountryCode.WesternSahara, "Western Sahara", "EH", "ESH", 212, "MAD", null),
        new(CountryCode.Yemen, "Yemen", "YE", "YEM", 967, "YER", null),
        new(CountryCode.Zambia, "Zambia", "ZM", "ZMB", 260, "ZMW", @"^\d{5}$"),
        new(CountryCode.Zimbabwe, "Zimbabwe", "ZW", "ZWE", 263, "ZWL", null),
    ];
}
