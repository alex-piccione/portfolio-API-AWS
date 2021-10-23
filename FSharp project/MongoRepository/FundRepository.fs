namespace Portfolio.MongoRepository

//open System.Linq
open Portfolio.Core
open Portfolio.Core.Entities
open MongoDB.Driver

open System.Linq
open System.Collections.Generic

type QueryType = {
    aa:string
    bb:string
    }

type FundRepository (connectionString:string) =
    inherit CrudRepository<FundAtDate>(connectionString, "Fund", (fun x -> x.Id))

    (*interface IFundRepository with
        member this.GetFundsToDate(date: System.DateTime): FundAtDate list = 
            // group by currency and get the latest
            let aggregate = base.Collection.Aggregate().Group((fun f -> f.CurrencyCode), fun g -> g.Where(fun g -> g.Date <= date).OrderBy(fun f -> f.Date).Last() )
            List.ofSeq( aggregate.ToEnumerable() )
            *)

    interface IFundRepository with
        member this.GetFundsToDate(date: System.DateTime): FundAtDate list = 

            let currencies = List.ofSeq (this.Collection.Distinct((fun x -> x.CurrencyCode), FilterDefinition<FundAtDate>.Empty).ToEnumerable())

            let getLast currencyCode = this.Collection.FindSync((fun f -> f.CurrencyCode = currencyCode && f.Date <= date))
                                           .ToEnumerable().OrderByDescending( (fun f -> f.Date))
                                           .FirstOrDefault()

            let data = currencies |> List.map (fun c -> getLast c) |> List.filter (fun x -> not(isNull(box x)))

            data

            //List.empty<FundAtDate>


    member this.GetFundsToDate_(date: System.DateTime): FundAtDate list = 

            // group by currency and get the latest
            let filter = FilterDefinitionBuilder<FundAtDate>().Lte((fun x -> x.Date), date)
            //List.ofSeq( base.Collection.FindAsync(filter).Result.ToEnumerable() )

            //let getLast fund = fun f -> 
            let getKey = fun f -> f.CurrencyCode // (fun f -> f.CurrencyCode)
            let orderByDate f = fun f -> f.Date
            // (fun g -> (List.ofSeq(g.).OrderBy(fun f -> f.Date).Last())
            //let aggregate = base.Collection.Aggregate().Group((fun f -> f.CurrencyCode), fun g -> g.OrderBy(fun f -> f.Date).Last() )
            //ProjectionDefinition<
            let group = fun x -> x.CurrencyCode
            let aggregate = base.Collection.Aggregate()//.Unwind(fun x -> x.) 
                                               //.Group( (fun f -> f.CurrencyCode), (fun g -> g.Select(fun x -> x).Distinct())) // Select(fun x -> x.FundCompanyId)))  //(fun g -> g. {aa="a"; bb="b"}))
                                               .Group( (fun f -> f.CurrencyCode), (fun g -> g ))  // {aa="a"; bb="b"}
                                               .ToList()
            List.empty<FundAtDate>
            //List.ofSeq(aggregate)
                                           //.ToEnumerable()
        //.Find(filter).Project(fun p -> {p.CurrencyCode}) .Aggregate().Group((fun f -> f.CurrencyCode), fun g -> g. (fun f -> f.Date).Last() )
        //let aggregate = base.Collection.Aggregate().Group((fun f -> f.CurrencyCode), fun g -> for x in  g.GetEnumerator() {   } (fun f -> f.Date > date).Take(1).First() )
        //aggregate.ToEnumerable()
        //List.ofSeq( aggregate.ToEnumerable() )

        (*
        var aggregation = collection.Aggregate()
        .Unwind<Smartphone, UnwindedSmartphone>(x => x.Properties)
        .Group(key => key.Property, g => new
        {
            Id = g.Key,
            Count = g.Count(),
            Articles = g.Select(x => new
            {
                Name = x.Name
            }).Distinct()
        })
        .SortBy(x => x.Id);
        *)


        //member this.Save(fund: FundAtDate): unit = 
        //    raise (System.NotImplementedException())



