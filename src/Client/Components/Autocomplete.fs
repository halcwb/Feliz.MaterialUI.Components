namespace Components

module Autocomplete =

    open Elmish
    open Feliz
    open Feliz.UseElmish
    open Feliz.MaterialUI

    let private init () = (), Cmd.none

    type private Msg =
        | Select of string
        | Clear

    let private update dispatch msg _ =
        match msg with
        | Select s ->
            (), Cmd.ofSub (fun _ -> s |> dispatch)
        | Clear ->
            (), Cmd.ofSub (fun _ -> "" |> dispatch)

    type Filter =
        | StartsWith
        | Contains

    type Props =
        {
            Dispatch : string -> unit
            Options : string list
            Label : string
            Filter : Filter
        }

    let props =
        {
            Dispatch = ignore
            Options = []
            Label = ""
            Filter = Contains
        }

    let private useStyles = Styles.makeStyles (fun theme styles ->
        {|

        |}
    )

    // It seems that options need to be an array of objects.
    // Make the objects optional to allow for the case of no
    // selected object
    let mapOptions sl =
        sl
        |> List.map (fun s ->
            {| label = s |}
            |> Some
        )
        |> List.toArray

    let applyToOption def f (o: {| label : string|} option) =
        match o with
        | Some o -> o.label |> f
        | None   -> def ()

    let private comp =
        React.functionComponent ("autocomplete", fun (props : Props) ->
            let _, dispatch = React.useElmish(init, update props.Dispatch, [||])
            let classes  = useStyles()

            Mui.autocomplete [
                autocomplete.options (props.Options |> mapOptions)
                autocomplete.getOptionLabel (applyToOption (fun _ -> "") id)
                autocomplete.autoComplete true
                autocomplete.renderInput (fun pars ->
                    Mui.textField [
                        yield! pars.felizProps
                        textField.label props.Label
                    ]
                )

                autocomplete.onChange (applyToOption (fun _ -> Clear |> dispatch) (Select >> dispatch))
                autocomplete.getOptionSelected (=)

                autocomplete.filterOptions (fun (options : {| label : string |} option[]) (state : string) ->
                    options
                    |> Array.filter (fun o ->
                        match o with
                        | Some o ->
                            let label = o.label.Trim().ToLower()
                            match props.Filter with
                            | StartsWith -> label.StartsWith(state)
                            | Contains   -> label.Contains(state)
                        | None   -> false
                    )
                )
            ]
        )

    let render props = comp (props)

