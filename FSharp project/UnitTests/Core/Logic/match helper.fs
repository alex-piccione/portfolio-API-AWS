module match_helper

open NUnit.Framework
open NUnit.Framework.Constraints

type MatchingResult = Result_Ok | Result_Error //| Result_NotValid

type matchResult<'a> (expected: MatchingResult) =
    inherit Constraints.EqualConstraint(expected)

    override this.ApplyTo actual =
        let pass = 
            match box actual with
            | :? Result<'a,string> as result -> 
                match result with
                | Ok c -> expected = Result_Ok
                | Error s -> expected = Result_Error
            | _ -> failwith "passed type must be Result<'a,string>"
        
        ConstraintResult(this, actual, pass)

(*
type matchOkResult<'a> (expected: Result<_,_>) =
    inherit Constraints.EqualConstraint(expected)
        
    override this.ApplyTo actual =
        let pass = 
            match box actual with
            | :? Result<'a, string> as result -> 
                match result with
                | expected -> true
                | _ -> failwith "Unexpected result"
            | _ -> failwith "passed type must be Result<?,string>"
                
        ConstraintResult(this, actual, pass)
*)