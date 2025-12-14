using System.Text.Json;

namespace Sandbox.Scryfall.Types;

public enum LegalStatus
{
	Legal,
	NotLegal,
	Restricted,
	Banned,
}

[JsonConverter( typeof( LegalitiesType.LegalitiesConverter ) )]
public class LegalitiesType
{
	public LegalStatus Standard { get; set; }
	public LegalStatus Future { get; set; }
	public LegalStatus Historic { get; set; }
	public LegalStatus Timeless { get; set; }
	public LegalStatus Gladiator { get; set; }
	public LegalStatus Pioneer { get; set; }
	public LegalStatus Modern { get; set; }
	public LegalStatus Legacy { get; set; }
	public LegalStatus Pauper { get; set; }
	public LegalStatus Vintage { get; set; }
	public LegalStatus Penny { get; set; }
	public LegalStatus Commander { get; set; }
	public LegalStatus Oathbreaker { get; set; }
	public LegalStatus StandardBrawl { get; set; }
	public LegalStatus Brawl { get; set; }
	public LegalStatus Alchemy { get; set; }
	public LegalStatus PauperCommander { get; set; }
	public LegalStatus Duel { get; set; }
	public LegalStatus OldSchool { get; set; }
	public LegalStatus Premodern { get; set; }
	public LegalStatus PreDH { get; set; }

	// -------------------------------
	// Full-object JSON converter
	// -------------------------------
	public class LegalitiesConverter : JsonConverter<LegalitiesType>
	{
		public override LegalitiesType Read(
			ref Utf8JsonReader reader,
			Type typeToConvert,
			JsonSerializerOptions options )
		{
			var element = JsonDocument.ParseValue( ref reader ).RootElement;
			var l = new LegalitiesType();

			foreach ( var prop in element.EnumerateObject() )
			{
				var val = prop.Value.GetString();

				LegalStatus legality = val switch
				{
					"legal" => LegalStatus.Legal,
					"not_legal" => LegalStatus.NotLegal,
					"restricted" => LegalStatus.Restricted,
					"banned" => LegalStatus.Banned,
					_ => LegalStatus.NotLegal
				};

				switch ( prop.Name )
				{
					case "standard": l.Standard = legality; break;
					case "future": l.Future = legality; break;
					case "historic": l.Historic = legality; break;
					case "timeless": l.Timeless = legality; break;
					case "gladiator": l.Gladiator = legality; break;
					case "pioneer": l.Pioneer = legality; break;
					case "modern": l.Modern = legality; break;
					case "legacy": l.Legacy = legality; break;
					case "pauper": l.Pauper = legality; break;
					case "vintage": l.Vintage = legality; break;
					case "penny": l.Penny = legality; break;
					case "commander": l.Commander = legality; break;
					case "oathbreaker": l.Oathbreaker = legality; break;
					case "standardbrawl": l.StandardBrawl = legality; break;
					case "brawl": l.Brawl = legality; break;
					case "alchemy": l.Alchemy = legality; break;
					case "paupercommander": l.PauperCommander = legality; break;
					case "duel": l.Duel = legality; break;
					case "oldschool": l.OldSchool = legality; break;
					case "premodern": l.Premodern = legality; break;
					case "predh": l.PreDH = legality; break;
				}
			}

			return l;
		}

		public override void Write(
			Utf8JsonWriter writer,
			LegalitiesType value,
			JsonSerializerOptions options )
		{
			writer.WriteStartObject();

			writer.WriteString( "standard", ToString( value.Standard ) );
			writer.WriteString( "future", ToString( value.Future ) );
			writer.WriteString( "historic", ToString( value.Historic ) );
			writer.WriteString( "timeless", ToString( value.Timeless ) );
			writer.WriteString( "gladiator", ToString( value.Gladiator ) );
			writer.WriteString( "pioneer", ToString( value.Pioneer ) );
			writer.WriteString( "modern", ToString( value.Modern ) );
			writer.WriteString( "legacy", ToString( value.Legacy ) );
			writer.WriteString( "pauper", ToString( value.Pauper ) );
			writer.WriteString( "vintage", ToString( value.Vintage ) );
			writer.WriteString( "penny", ToString( value.Penny ) );
			writer.WriteString( "commander", ToString( value.Commander ) );
			writer.WriteString( "oathbreaker", ToString( value.Oathbreaker ) );
			writer.WriteString( "standardbrawl", ToString( value.StandardBrawl ) );
			writer.WriteString( "brawl", ToString( value.Brawl ) );
			writer.WriteString( "alchemy", ToString( value.Alchemy ) );
			writer.WriteString( "paupercommander", ToString( value.PauperCommander ) );
			writer.WriteString( "duel", ToString( value.Duel ) );
			writer.WriteString( "oldschool", ToString( value.OldSchool ) );
			writer.WriteString( "premodern", ToString( value.Premodern ) );
			writer.WriteString( "predh", ToString( value.PreDH ) );

			writer.WriteEndObject();
		}

		private static string ToString( LegalStatus l ) =>
			l switch
			{
				LegalStatus.Legal => "legal",
				LegalStatus.NotLegal => "not_legal",
				LegalStatus.Restricted => "restricted",
				LegalStatus.Banned => "banned",
				_ => "not_legal"
			};
	}
}
