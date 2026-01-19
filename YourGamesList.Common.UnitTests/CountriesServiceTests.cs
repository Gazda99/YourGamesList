using System;
using System.Collections.Generic;
using AutoFixture;

namespace YourGamesList.Common.UnitTests;

public class CountriesServiceTests
{
    private IFixture _fixture;

    private readonly Dictionary<string, string> _isoCodeToFullNameMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _fullNameToIsoCodeMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        for (var i = 0; i < IsoCodes.Length; i++)
        {
            var isoCode = IsoCodes[i];
            var fullName = FullEnglishNames[i];

            _isoCodeToFullNameMap[isoCode] = fullName;
            _fullNameToIsoCodeMap[fullName] = isoCode;
        }
    }

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture();
    }

    [Test]
    [TestCaseSource(nameof(FullEnglishNames))]
    public void ValidateFullEnglishName_ForValidName_ReturnsTrue(string fullEnglishName)
    {
        //ARRANGE
        var countriesService = new CountriesService();

        //ACT
        var res = countriesService.ValidateFullEnglishName(fullEnglishName);

        //ASSERT
        Assert.That(res, Is.True);
    }

    [Test]
    [TestCaseSource(nameof(IsoCodes))]
    public void ValidateThreeLetterIsoRegionName_ForValid_NameReturnsTrue(string isoCode)
    {
        //ARRANGE
        var countriesService = new CountriesService();

        //ACT
        var res = countriesService.ValidateThreeLetterIsoCode(isoCode);

        //ASSERT
        Assert.That(res, Is.True);
    }

    [Test]
    public void ValidateFullEnglishName_ForInvalidName_ReturnsTrue()
    {
        //ARRANGE
        var fullEnglishName = _fixture.Create<string>();

        var countriesService = new CountriesService();

        //ACT
        var res = countriesService.ValidateFullEnglishName(fullEnglishName);
        //ASSERT
        Assert.That(res, Is.False);
    }

    [Test]
    public void ValidateThreeLetterIsoRegionName_ForInvalid_NameReturnsTrue()
    {
        //ARRANGE
        var isoCode = _fixture.Create<string>();

        var countriesService = new CountriesService();

        //ACT
        var res = countriesService.ValidateThreeLetterIsoCode(isoCode);

        //ASSERT
        Assert.That(res, Is.False);
    }

    [Test]
    [TestCaseSource(nameof(FullEnglishNames))]
    public void TryParseFullEnglishName_ForValidName_ParsesSuccessfully(string fullEnglishName)
    {
        //ARRANGE
        var countriesService = new CountriesService();

        //ACT
        var res = countriesService.TryParseFullEnglishName(fullEnglishName, out var isoCode);

        //ASSERT
        Assert.That(res, Is.True);
        Assert.That(isoCode, Is.EquivalentTo(_fullNameToIsoCodeMap[fullEnglishName]));
    }

    [Test]
    [TestCaseSource(nameof(IsoCodes))]
    public void TryParseThreeLetterIsoRegionName_ForValidIsoCode_ParsesSuccessfully(string isoCode)
    {
        //ARRANGE
        var countriesService = new CountriesService();

        //ACT
        var res = countriesService.TryParseThreeLetterIsoRegionName(isoCode, out var fullEnglishName);

        //ASSERT
        Assert.That(res, Is.True);
        Assert.That(fullEnglishName, Is.EquivalentTo(_isoCodeToFullNameMap[isoCode]));
    }

    [Test]
    public void TryParseFullEnglishName_ForInvalidName_DoesNotParse()
    {
        //ARRANGE
        var fullEnglishName = _fixture.Create<string>();
        var countriesService = new CountriesService();

        //ACT
        var res = countriesService.TryParseFullEnglishName(fullEnglishName, out var _);

        //ASSERT
        Assert.That(res, Is.False);
    }

    [Test]
    public void TryParseThreeLetterIsoRegionName_ForInvalidIsoCode_DoesNotParse()
    {
        //ARRANGE
        var isoCode = _fixture.Create<string>();
        var countriesService = new CountriesService();

        //ACT
        var res = countriesService.TryParseThreeLetterIsoRegionName(isoCode, out var _);

        //ASSERT
        Assert.That(res, Is.False);
    }

    private static readonly string[] IsoCodes =
    [
        "ABW",
        "AFG",
        "AGO",
        "AIA",
        "ALA",
        "ALB",
        "AND",
        "ARE",
        "ARG",
        "ARM",
        "ASM",
        "ATG",
        "AUS",
        "AUT",
        "AZE",
        "BDI",
        "BEL",
        "BEN",
        "BES",
        "BFA",
        "BGD",
        "BGR",
        "BHR",
        "BHS",
        "BIH",
        "BLM",
        "BLR",
        "BLZ",
        "BMU",
        "BOL",
        "BRA",
        "BRB",
        "BRN",
        "BTN",
        "BWA",
        "CAF",
        "CAN",
        "CCK",
        "CHE",
        "CHL",
        "CHN",
        "CIV",
        "CMR",
        "COD",
        "COG",
        "COK",
        "COL",
        "COM",
        "CPV",
        "CRI",
        "CUB",
        "CUW",
        "CXR",
        "CYM",
        "CYP",
        "CZE",
        "DEU",
        "DJI",
        "DMA",
        "DNK",
        "DOM",
        "DZA",
        "ECU",
        "EGY",
        "ERI",
        "ESP",
        "EST",
        "ETH",
        "FIN",
        "FJI",
        "FLK",
        "FRA",
        "FRO",
        "FSM",
        "GAB",
        "GBR",
        "GEO",
        "GGY",
        "GHA",
        "GIB",
        "GIN",
        "GLP",
        "GMB",
        "GNB",
        "GNQ",
        "GRC",
        "GRD",
        "GRL",
        "GTM",
        "GUF",
        "GUM",
        "GUY",
        "HKG",
        "HND",
        "HRV",
        "HTI",
        "HUN",
        "IDN",
        "IMN",
        "IND",
        "IOT",
        "IRL",
        "IRN",
        "IRQ",
        "ISL",
        "ISR",
        "ITA",
        "JAM",
        "JEY",
        "JOR",
        "JPN",
        "KAZ",
        "KEN",
        "KGZ",
        "KHM",
        "KIR",
        "KNA",
        "KOR",
        "KWT",
        "LAO",
        "LBN",
        "LBR",
        "LBY",
        "LCA",
        "LIE",
        "LKA",
        "LSO",
        "LTU",
        "LUX",
        "LVA",
        "MAC",
        "MAF",
        "MAR",
        "MCO",
        "MDA",
        "MDG",
        "MDV",
        "MEX",
        "MHL",
        "MKD",
        "MLI",
        "MLT",
        "MMR",
        "MNE",
        "MNG",
        "MNP",
        "MOZ",
        "MRT",
        "MSR",
        "MTQ",
        "MUS",
        "MWI",
        "MYS",
        "MYT",
        "NAM",
        "NCL",
        "NER",
        "NFK",
        "NGA",
        "NIC",
        "NIU",
        "NLD",
        "NOR",
        "NPL",
        "NRU",
        "NZL",
        "OMN",
        "PAK",
        "PAN",
        "PCN",
        "PER",
        "PHL",
        "PLW",
        "PNG",
        "POL",
        "PRI",
        "PRK",
        "PRT",
        "PRY",
        "PSE",
        "PYF",
        "QAT",
        "REU",
        "ROU",
        "RUS",
        "RWA",
        "SAU",
        "SDN",
        "SEN",
        "SGP",
        "SHN",
        "SJM",
        "SLB",
        "SLE",
        "SLV",
        "SMR",
        "SOM",
        "SPM",
        "SRB",
        "SSD",
        "STP",
        "SUR",
        "SVK",
        "SVN",
        "SWE",
        "SWZ",
        "SXM",
        "SYC",
        "SYR",
        "TCA",
        "TCD",
        "TGO",
        "THA",
        "TJK",
        "TKL",
        "TKM",
        "TLS",
        "TON",
        "TTO",
        "TUN",
        "TUR",
        "TUV",
        "TWN",
        "TZA",
        "UGA",
        "UKR",
        "UMI",
        "URY",
        "USA",
        "UZB",
        "VAT",
        "VCT",
        "VEN",
        "VGB",
        "VIR",
        "VNM",
        "VUT",
        "WLF",
        "WSM",
        "XKK",
        "YEM",
        "ZAF",
        "ZMB",
        "ZWE"
    ];

    private static readonly string[] FullEnglishNames =
    [
        "Aruba",
        "Afghanistan",
        "Angola",
        "Anguilla",
        "Åland Islands",
        "Albania",
        "Andorra",
        "United Arab Emirates",
        "Argentina",
        "Armenia",
        "American Samoa",
        "Antigua & Barbuda",
        "Australia",
        "Austria",
        "Azerbaijan",
        "Burundi",
        "Belgium",
        "Benin",
        "Bonaire, Sint Eustatius and Saba",
        "Burkina Faso",
        "Bangladesh",
        "Bulgaria",
        "Bahrain",
        "Bahamas",
        "Bosnia & Herzegovina",
        "St. Barthélemy",
        "Belarus",
        "Belize",
        "Bermuda",
        "Bolivia",
        "Brazil",
        "Barbados",
        "Brunei",
        "Bhutan",
        "Botswana",
        "Central African Republic",
        "Canada",
        "Cocos (Keeling) Islands",
        "Switzerland",
        "Chile",
        "China",
        "Côte d'Ivoire",
        "Cameroon",
        "Congo (DRC)",
        "Congo",
        "Cook Islands",
        "Colombia",
        "Comoros",
        "Cabo Verde",
        "Costa Rica",
        "Cuba",
        "Curaçao",
        "Christmas Island",
        "Cayman Islands",
        "Cyprus",
        "Czechia",
        "Germany",
        "Djibouti",
        "Dominica",
        "Denmark",
        "Dominican Republic",
        "Algeria",
        "Ecuador",
        "Egypt",
        "Eritrea",
        "Spain",
        "Estonia",
        "Ethiopia",
        "Finland",
        "Fiji",
        "Falkland Islands",
        "France",
        "Faroe Islands",
        "Micronesia",
        "Gabon",
        "United Kingdom",
        "Georgia",
        "Guernsey",
        "Ghana",
        "Gibraltar",
        "Guinea",
        "Guadeloupe",
        "Gambia",
        "Guinea-Bissau",
        "Equatorial Guinea",
        "Greece",
        "Grenada",
        "Greenland",
        "Guatemala",
        "French Guiana",
        "Guam",
        "Guyana",
        "Hong Kong SAR",
        "Honduras",
        "Croatia",
        "Haiti",
        "Hungary",
        "Indonesia",
        "Isle of Man",
        "India",
        "British Indian Ocean Territory",
        "Ireland",
        "Iran",
        "Iraq",
        "Iceland",
        "Israel",
        "Italy",
        "Jamaica",
        "Jersey",
        "Jordan",
        "Japan",
        "Kazakhstan",
        "Kenya",
        "Kyrgyzstan",
        "Cambodia",
        "Kiribati",
        "St. Kitts & Nevis",
        "Korea",
        "Kuwait",
        "Laos",
        "Lebanon",
        "Liberia",
        "Libya",
        "St. Lucia",
        "Liechtenstein",
        "Sri Lanka",
        "Lesotho",
        "Lithuania",
        "Luxembourg",
        "Latvia",
        "Macao SAR",
        "St. Martin",
        "Morocco",
        "Monaco",
        "Moldova",
        "Madagascar",
        "Maldives",
        "Mexico",
        "Marshall Islands",
        "North Macedonia",
        "Mali",
        "Malta",
        "Myanmar",
        "Montenegro",
        "Mongolia",
        "Northern Mariana Islands",
        "Mozambique",
        "Mauritania",
        "Montserrat",
        "Martinique",
        "Mauritius",
        "Malawi",
        "Malaysia",
        "Mayotte",
        "Namibia",
        "New Caledonia",
        "Niger",
        "Norfolk Island",
        "Nigeria",
        "Nicaragua",
        "Niue",
        "Netherlands",
        "Norway",
        "Nepal",
        "Nauru",
        "New Zealand",
        "Oman",
        "Pakistan",
        "Panama",
        "Pitcairn Islands",
        "Peru",
        "Philippines",
        "Palau",
        "Papua New Guinea",
        "Poland",
        "Puerto Rico",
        "North Korea",
        "Portugal",
        "Paraguay",
        "Palestinian Authority",
        "French Polynesia",
        "Qatar",
        "Réunion",
        "Romania",
        "Russia",
        "Rwanda",
        "Saudi Arabia",
        "Sudan",
        "Senegal",
        "Singapore",
        "St Helena, Ascension, Tristan da Cunha",
        "Svalbard & Jan Mayen",
        "Solomon Islands",
        "Sierra Leone",
        "El Salvador",
        "San Marino",
        "Somalia",
        "St. Pierre & Miquelon",
        "Serbia",
        "South Sudan",
        "Sao Tomé & Príncipe",
        "Suriname",
        "Slovakia",
        "Slovenia",
        "Sweden",
        "Eswatini",
        "Sint Maarten",
        "Seychelles",
        "Syria",
        "Turks & Caicos Islands",
        "Chad",
        "Togo",
        "Thailand",
        "Tajikistan",
        "Tokelau",
        "Turkmenistan",
        "Timor-Leste",
        "Tonga",
        "Trinidad & Tobago",
        "Tunisia",
        "Türkiye",
        "Tuvalu",
        "Taiwan",
        "Tanzania",
        "Uganda",
        "Ukraine",
        "U.S. Outlying Islands",
        "Uruguay",
        "United States",
        "Uzbekistan",
        "Vatican City",
        "St. Vincent & Grenadines",
        "Venezuela",
        "British Virgin Islands",
        "U.S. Virgin Islands",
        "Vietnam",
        "Vanuatu",
        "Wallis & Futuna",
        "Samoa",
        "Kosovo",
        "Yemen",
        "South Africa",
        "Zambia",
        "Zimbabwe",
    ];
}