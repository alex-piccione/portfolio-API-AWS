module serializers

open MongoDB.Bson.Serialization
open Portfolio.Core.Entities

type CompanyTypeSerializer () =

    interface IBsonSerializer with

        member this.Deserialize(context: BsonDeserializationContext, args: BsonDeserializationArgs): obj = 

            let mutable types = List.empty<CompanyType>

            context.Reader.ReadStartArray()
            while context.Reader.ReadBsonType() <> MongoDB.Bson.BsonType.EndOfDocument do
                let value = CompanyType.Parse (context.Reader.ReadString())
                types <- value::types

            context.Reader.ReadEndArray()
            box types

        member this.Serialize(context: BsonSerializationContext, args: BsonSerializationArgs, value: obj): unit = 
            context.Writer.WriteStartArray()
            value :?> CompanyType list |> List.iter (fun i -> context.Writer.WriteString (string i))
            context.Writer.WriteEndArray()

        member this.ValueType: System.Type = typeof<CompanyType list>
