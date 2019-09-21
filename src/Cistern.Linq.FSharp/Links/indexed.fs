﻿namespace Cistern.Linq.FSharp.Links

open Cistern.Linq.ChainLinq

type IndexedActivity<'T>(next) =
    inherit Cistern.Linq.ChainLinq.Activity<'T,int*'T>(next)

    let mutable idx = -1

    override __.ProcessNext input =
        idx <- idx + 1
        base.Next (idx, input)

type Indexed<'T> private () =
    static let instance = Indexed<'T> ()

    static member Instance = instance

    interface Cistern.Linq.ChainLinq.ILink<'T, int*'T> with
        member __.Compose activity = 
            upcast IndexedActivity(activity)
