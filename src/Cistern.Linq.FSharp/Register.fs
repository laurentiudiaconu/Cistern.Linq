﻿module Cistern.Linq.FSharp.Register

open Cistern.Linq
open System.Collections.Generic

type private TryFindImmutableTypes () =
    interface Registry.ITryFindSpecificType with
        member __.Namespace = "Microsoft.FSharp.Collections"

        member __.TryCreateSpecific<'T, 'U, 'Construct when 'Construct :> Registry.IConstruct<'T, 'U>> (construct:'Construct, e:IEnumerable<'T>, name:string) =
            if name.Length <= 6 then null
            else
                let sixthChar = name.[6] //  here  |
                                         //       \|/
                                         // 'FSharpXXXX'
                                         //  0123456789
                if sixthChar = 'L' then
                    match e with
                    | :? list<'T> as l -> construct.Create (TypedEnumerables.FSharpListEnumerable<'T> l)
                    | _ -> null
                else 
                    null

        member __.TryInvoke<'T, 'Invoker when 'Invoker :> Registry.IInvoker<'T>> (invoker:'Invoker, e:IEnumerable<'T>, name:string) =
            if name.Length <= 6 then false
            else
                let sixthChar = name.[6] //  here  |
                                         //       \|/
                                         // 'FSharpXXXX'
                                         //  0123456789
                if sixthChar = 'L' then
                    match e with
                    | :? list<'T> as l -> invoker.Invoke (TypedEnumerables.FSharpListEnumerable<'T> l); true
                    | _ -> false
                else 
                    false

module private TryFindImmutableTypesInstance =
    let Instance = TryFindImmutableTypes () :> Registry.ITryFindSpecificType

let RegisterFSharpCollections () =
    Registry.Register TryFindImmutableTypesInstance.Instance