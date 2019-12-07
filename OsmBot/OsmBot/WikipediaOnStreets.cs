using System.Collections.Generic;
using OsmSharp.Complete;

namespace OsmBot
{
    /// <summary>
    /// Adds a wikipedia tag onto streets of Bruges, based on a published list on wikipedia itself:
    /// https://nl.wikipedia.org/wiki/Lijst_van_straten_en_pleinen_in_Brugge
    ///
    /// The special cases (where the name of the street has an additional "(Brugge)" are split below    
    ///
    /// 
    /// </summary>
    public class WikipediaForBruges
    {
        private readonly EasyChangeset _cs;

        public WikipediaForBruges(EasyChangeset cs)
        {
            _cs = cs;
        }


        public void ApplyWikipediaTag(CompleteOsmGeo geo)
        {
            var name = geo.Tags.GetValue("name");
            if (string.IsNullOrEmpty(name))
            {
                return;
            }

            if (BruggeSpecifiek.Contains(name))
            {
                geo.AddNewTag("wikipedia", $"nl:{name} (Brugge)");
                _cs.AddChange(geo);
            }
            else if (GewoneStraten.Contains(name))
            {
                geo.AddNewTag("wikipedia", $"nl:{name}");
                _cs.AddChange(geo);
            }
        }

        static HashSet<string> BruggeSpecifiek = new HashSet<string>
        {
            "Bakkersstraat", "Beenhouwersstraat", "Begijnenvest", "Beursplein", "Burg", "Burgstraat", "Eiland",
            "Garenmarkt", "De Garre", "'s-Gravenhof", "Groenestraat", "Grote Markt", "Heilige-Geeststraat",
            "Hemelrijk", "Hoogstraat", "Ieperstraat", "Kapelstraat", "Kipstraat", "Klokstraat", "Koningstraat",
            "Lange Raamstraat", "Lane", "Lange Raamstraat", "Langestraat", "Mariastraat", "Minderbroedersstraat",
            "Muntplein", "Muntpoort", "Nieuwstraat", "Noordstraat", "Oranjeboomstraat", "Park", "Peperstraat",
            "Predikherenstraat", "Prinsenhof", "Raamstraat", "Rodestraat", "Rozendal", "Sarepta",
            "Simon Stevinplein", "Sint-Jansplein", "Sint-Jansstraat", "Stationsplein", "Steenstraat",
            "Stoofstraat", "Twijnstraat", "'t Zand", "Vismarkt", "Vrijdagmarkt", "Walstraat", "Wijngaardstraat",
            "Willemstraat"
        };

        static HashSet<string> GewoneStraten = new HashSet<string>
        {
            "Academiestraat", "Achiel Van Ackerplein", "Adriaan Willaertstraat", "Albrecht Rodenbachstraat",
            "Ankerplein", "Annuntiatenstraat", "Arsenaalstraat", "Artoisstraat", "Augustijnenrei", "Azijnstraat",
            "Baliestraat", "Balsemboomstraat", "Balstraat", "Bapaumestraat", "Bargeweg", "Biddersstraat",
            "Biezenstraat", "Bilkske", "Biskajersplein", "Blekersstraat", "Blinde-Ezelstraat", "Bloedput", "Blokstraat",
            "Boeveriestraat", "Bollaardstraat", "Boninvest", "Boomgaardstraat", "Boterhuis", "Boudewijn Ostenstraat",
            "Boudewijn Ravestraat", "Braambergstraat", "Brandstraat", "Breidelstraat", "Buiten Begijnenvest",
            "Buiten Boeverievest", "Buiten Boninvest", "Buiten de Smedenpoort", "Buiten Kazernevest",
            "Buiten Kruisvest", "Buiten Smedenvest", "Calvariebergstraat", "Carmersstraat", "Colettijnenhof",
            "Colettijnenstraat", "Collaert Mansionstraat", "Cordoeaniersstraat", "Diamantslijpersstraat", "Dijver",
            "Driekroezenstraat", "Drie Zwanenstraat", "Duinenabdijstraat", "Dweersstraat", "Eekhoutpoort",
            "Eekhoutstraat", "Eiermarkt", "Elf-Julistraat", "Elisabeth Zorghestraat", "Engelsestraat", "Engelstraat",
            "Essenboomstraat", "Ezelpoort (straat)", "Ezelstraat", "Fonteinstraat", "Freren Fonteinstraat",
            "Ganzenplein", "Ganzenstraat", "Gapaardstraat", "Garsoenstraat", "Geernaartstraat", "Geerolfstraat",
            "Geerwijnstraat", "Geldmuntstraat", "Genthof", "Gentpoortstraat", "Gentpoortvest", "Gevangenisstraat",
            "Gieterijstraat", "Gistelhof", "Giststraat", "Gloribusstraat", "Goezeputstraat", "Gotje", "Gouden-Handrei",
            "Gouden-Handstraat", "Goudsmedenstraat", "Grauwwerkersstraat", "Greinschuurstraat", "Groenerei",
            "Groeninge", "Gruuthusestraat", "Guido Gezellelaan", "Guido Gezelleplein", "Guido Gezellewarande",
            "Gulden-Vlieslaan", "Haanstraat", "Haarakkerstraat", "Hallestraat", "Hauwerstraat", "Helmstraat",
            "Hendrik Consciencelaan", "Hendrik Pulinxpad", "Hertsbergestraat", "Hoedenmakersstraat", "Hoefijzerlaan",
            "Hoogste van Brugge", "Hoogstuk", "Hooistraat", "Hoornstraat", "Hugo Losschaertstraat",
            "Hugo Verrieststraat", "Huidenvettersplein", "{{Inhoud abc}}", "Jakobinessenstraat", "Jakob van Ooststraat",
            "James Wealestraat", "Jan Boninstraat", "Jan Miraelstraat", "Jan van Eyckplein", "Jeruzalemstraat",
            "Joost de Damhouderstraat", "Joris Dumeryplein", "Jozef Suvéestraat", "Julius en Maurits Sabbestraat",
            "Kalkovenstraat", "Kammakersstraat", "Kandelaarstraat", "Kantwerkstersplein", "Kartuizerinnenstraat",
            "Kastanjeboomstraat", "Katelijnestraat", "Katelijnevest", "Kazernevest", "Keersstraat", "Kegelschoolstraat",
            "Kelkstraat", "Kemelstraat", "Kersenboomstraat", "Klaverstraat", "Kleine Heilige-Geeststraat",
            "Kleine Hertsbergestraat", "Kleine Hoedenmakersstraat", "Kleine Hoefijzerstraat", "Kleine Kuipersstraat",
            "Kleine Nieuwstraat", "Kleine Sint-Amandsstraat", "Kleine Sint-Jansstraat", "Komvest", "Konfijtstraat",
            "Koning Albert I-laan", "Koningin Elisabethlaan", "Koolbrandersstraat", "Koopmansstraat", "Kopstraat",
            "Korte Lane", "Korte Riddersstraat", "Korte Rijkepijndersstraat", "Korte Ropeerdstraat",
            "Korte Sint-Annastraat", "Korte Speelmansstraat", "Korte Vuldersstraat", "Kortewinkel", "Koudemarkt",
            "Kraanplein", "Kraanrei", "Kreupelenstraat", "Krom Genthof", "Kruiersstraat", "Kruisvest",
            "Kruitenbergstraat", "Kuipersstraat", "Kwekersstraat", "Langerei", "Leemputstraat", "Leestenburg",
            "Leeuwstraat", "Leffingestraat", "Lendestraat", "Loppemstraat", "Maagdendal", "Maagdenstraat",
            "Mallebergplaats", "Meestraat", "Middelburgstraat", "Minneboplein", "Moerkerkestraat", "Moerstraat",
            "Molenmeers", "Mortierstraat", "Naaldenstraat", "Neststraat", "Nieuwe Gentweg", "Niklaas Desparsstraat",
            "Noordzandstraat", "Oliebaan", "Ontvangersstraat", "Onze-Lieve-Vrouwekerkhof-Zuid", "Oosterlingenplein",
            "Oostmeers", "Oostproosse", "Oude Burg", "Oude Gentweg", "Oude Zak", "Oude Zomerstraat", "Paalstraat",
            "Palmstraat", "Pandreitje", "Papegaaistraat", "Pastoor Van Haeckeplantsoen", "Pater Damiaanstraat",
            "Peerdenstraat", "Peterseliestraat", "Philipstockstraat", "Pieter Pourbusstraat", "Pijpersstraat",
            "Poitevinstraat", "Pottenmakersstraat", "Potterierei", "Predikherenrei", "Professor Dr. J. Sebrechtsstraat",
            "Riddersstraat", "Rijkepijndersstraat", "Robijnstraat", "Rode-Haanstraat", "Rolweg", "Roompotstraat",
            "Ropeerdstraat", "Rozemarijnstraat", "Rozenhoedkaai", "Sasplein", "Schaarstraat", "Schottinnenstraat",
            "Schouwvegersstraat", "Schrijnwerkersstraat", "Schrijversstraat", "Schuttersstraat", "Sentillenhof",
            "'s-Gravenstraat", "'s-Gravenstraat", "Sint-Amandsstraat", "Sint-Annakerkstraat", "Sint-Annaplein",
            "Sint-Annarei", "Sint-Brunostraat", "Sint-Claradreef", "Sint-Clarastraat", "Sint-Gillisdorpstraat",
            "Sint-Gilliskerkhof", "Sint-Gilliskerkstraat", "Sint-Gilliskoorstraat", "Sint-Jakobsplein",
            "Sint-Jakobsstraat", "Sint-Jan in de Meers", "Sint-Jorisstraat", "Sint-Maartensbilk", "Sint-Maartensplein",
            "Sint-Niklaasstraat", "Sint-Obrechtsstraat", "Sint-Salvatorskerkhof", "Sint-Salvatorskoorstraat",
            "Sint-Walburgastraat", "Sledestraat", "Smedenstraat", "Snaggaardstraat", "Spaanse Loskaai",
            "Spanjaardstraat", "Speelmansrei", "Speelmansstraat", "Spiegelrei", "Spikkelboorstraat", "Spinolarei",
            "Stalijzerstraat", "Steenhouwersdijk", "Sterstraat", "Stijn Streuvelsstraat", "Stoelstraat",
            "Stokersstraat", "Strostraat", "Sulferbergstraat", "Timmermansstraat", "Torenbrug", "'t Pand", "'t Zweerd",
            "Unescorotonde", "Van Voldenstraat", "Venkelstraat", "Verbrand Nieuwland", "Verversdijk", "Violierstraat",
            "Visspaanstraat", "Vizierstraat", "Vlamingdam", "Vlamingstraat", "Vuldersreitje", "Vuldersstraat",
            "Waalsestraat", "Walplein", "Walweinstraat", "Wapenmakersstraat", "Werkhuisstraat", "Westmeers",
            "Wevershof", "Wijngaardplein", "Wijnzakstraat", "Willem de Dekenstraat", "Willemijnendreef",
            "Witteleertouwersstraat", "Woensdagmarkt", "Wollestraat", "Wulfhagestraat", "Wulpenstraat", "Zakske",
            "Zevensterrestraat", "Zilverpand", "Zilversteeg", "Zilverstraat", "Zonnekemeers", "Zuidzandstraat",
            "Zwarteleertouwersstraat", "Zwijnstraat"
        };
    }
}