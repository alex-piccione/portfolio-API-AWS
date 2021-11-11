module helper

open System
open NUnit.Framework
open NUnit.Framework.Constraints
open FsUnit

let dateTolerance = TimeSpan.FromSeconds(1.)

type equalDate(expected:DateTime) = 
    inherit Constraints.EqualConstraint(expected)

    override this.ApplyTo<'C> (actual: 'C):  ConstraintResult =
        match box actual with 
        | :? DateTime as date ->
            date |> should (equalWithin dateTolerance) expected
            ConstraintResult(this, actual, true)
        | _ ->
            ConstraintResult(this, actual, false)

