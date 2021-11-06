module AWSRequestSigner
(* 
AWS request signer V4
https://docs.aws.amazon.com/general/latest/gr/sigv4-create-canonical-request.html
*)

open System
open System.Text
open System.Linq
open System.Collections.Generic
open System.Security.Cryptography
open System.Web

module private helper =

    let ToHexString(array:IReadOnlyCollection<byte>) =
        let hex = StringBuilder(array.Count * 2)
        for b in array do 
            hex.AppendFormat("{0:x2}", b) |> ignore
        hex.ToString()

    let Hash(bytesToHash:byte[]) = 
        using (SHA256.Create()) (fun sha256 ->
            ToHexString(sha256.ComputeHash(bytesToHash))
        )

    let HmacSha256(key, data) =
        let hashAlgorithm = new HMACSHA256(key)
        hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(data:string))

    let GetSignatureKey(key, dateStamp, regionName, serviceName) =
        let kSecret = Encoding.UTF8.GetBytes("AWS4" + key)
        let kDate = HmacSha256(kSecret, dateStamp)
        let kRegion = HmacSha256(kDate, regionName)
        let kService = HmacSha256(kRegion, serviceName)
        HmacSha256(kService, "aws4_request")

    let GetCanonicalQueryParams(queryString:string) =

        let values = SortedDictionary<string, IEnumerable<string>>(StringComparer.Ordinal)

        let querystring = HttpUtility.ParseQueryString(queryString)
        for key in querystring.AllKeys do

            let escapedKey = Uri.EscapeDataString(key)
            let value = querystring.[key]
            if value = null 
            then 
                values.Add(escapedKey, [| $"{escapedKey}=" |])
            else
                // Handles multiple values per query parameter
                let queryValues = 
                    value.Split(',')
                        // Order by value alphanumerically (required for correct canonical string)
                        .OrderBy(fun v -> v)
                        // Query params must be escaped in upper case (i.e. "%2C", not "%2c").
                        .Select(fun v -> $"{escapedKey}={Uri.EscapeDataString(v)}");

                values.Add(escapedKey, queryValues)

        let queryParams = values.SelectMany(fun a -> a.Value)
        String.Join("&", queryParams)


open helper
let ALGORITHM = "AWS4-HMAC-SHA256"
let createAuhtorizationHeaders(method:string, path:string, queryString:string, content:string, headers:IDictionary<string, string>, service:string, region:string) = //timeOffset:Option<TimeSpan> 
    
        if String.IsNullOrEmpty(secrets.accessKey) then
            raise (ArgumentNullException("accessKey", "Not a valid access_key."))

        if String.IsNullOrEmpty(secrets.secretKey) then
            raise(ArgumentNullException("secretKey", "Not a valid secret_key."))

        if String.IsNullOrEmpty(service) then 
            raise(ArgumentOutOfRangeException(nameof(service), service, "Not a valid service."))

        if String.IsNullOrEmpty(region) then
            raise(ArgumentOutOfRangeException(nameof(region), region, "Not a valid region."))


        let newHeaders = Array.create<(string * string)> 3 ("", "")

        let contentBytes = if content.Length > 0 then Encoding.UTF8.GetBytes(content) else Array.Empty<byte>()
        let hashedContent = Hash(contentBytes)
        newHeaders.[0] <- ("x-amz-content-sha256", hashedContent)

        let time = DateTimeOffset.UtcNow

        let dateAndTime = time.ToString("yyyyMMddTHHmmssZ")
        newHeaders.[1] <- ("x-amz-date", dateAndTime)

        let date = time.ToString("yyyyMMdd")

        let canonicalRequest = StringBuilder()
        canonicalRequest.Append $"{method}\n" |> ignore
       
        canonicalRequest.Append (String.Join("/", path.Split('/').Select(Uri.EscapeDataString)) + "\n") |> ignore

        let canonicalQueryParams = GetCanonicalQueryParams(queryString)

        canonicalRequest.Append (canonicalQueryParams + "\n") |> ignore
        
        let signedHeadersList = List<string>()

        let headersToSign = (headers.Concat(newHeaders.Where( fun (h,v) -> h <> "") |> dict)).ToDictionary((fun kv -> kv.Key), fun kv -> kv.Value)

        for header in headersToSign.OrderBy(fun a -> a.Key.ToLowerInvariant(), StringComparer.OrdinalIgnoreCase) do
            canonicalRequest.Append (header.Key.ToLowerInvariant()) |> ignore
            canonicalRequest.Append ":" |> ignore
            // TODO manage multivalue headers
            // canonicalRequest.Append (String.Join(",", header.Value.Select(fun s -> s.Trim()))) |> ignore
            canonicalRequest.Append header.Value |> ignore
            canonicalRequest.Append "\n" |> ignore
            signedHeadersList.Add(header.Key.ToLowerInvariant())

        canonicalRequest.Append("\n") |> ignore

        let signedHeaders = String.Join(";", signedHeadersList)

        canonicalRequest.Append (signedHeaders + "\n") |> ignore
        canonicalRequest.Append hashedContent |> ignore

        let credentialScope = $"{date}/{region}/{service}/aws4_request"

        let stringToSign = $"{ALGORITHM}\n{dateAndTime}\n{credentialScope}\n" + Hash(Encoding.UTF8.GetBytes(canonicalRequest.ToString()))

        let signingKey = GetSignatureKey(secrets.secretKey, date , region, service)
        let signature = ToHexString(HmacSha256(signingKey, stringToSign))

        newHeaders.[2] <- ("Authorization", $"{ALGORITHM} Credential={secrets.accessKey}/{credentialScope}, SignedHeaders={signedHeaders}, Signature={signature}")
        newHeaders