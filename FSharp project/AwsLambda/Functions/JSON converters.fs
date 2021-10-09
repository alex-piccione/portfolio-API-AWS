namespace Portfolio.Api.Functions

open System.Text.Json
open System.Text.Json.Serialization
open Portfolio.Core.Entities

type internal CompanyTypesJsonConverter () =
    inherit JsonConverter<CompanyType list>()

    override this.Read(reader, typeToConvert, options) =
        let mutable values = List.Empty
        while reader.Read() && reader.TokenType <> JsonTokenType.EndArray do
            match reader.TokenType with 
            | JsonTokenType.String -> 
                values <- CompanyType.Parse(reader.GetString())::values
            | _ -> ()
        values

    override this.Write(writer, value, options) =
        writer.WriteStartArray()
        value |> List.iter (fun item -> writer.WriteStringValue (item.ToString())) 
        writer.WriteEndArray()