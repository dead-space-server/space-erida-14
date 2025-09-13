shared-solution-container-component-on-examine-empty-container = Не содержит вещества.
shared-solution-container-component-on-examine-main-text = Содержит [color={ $color }]{ $desc }[/color] { $chemCount ->
    [1] вещество.
   *[other] смесь из веществ.
    }
shared-solution-container-component-on-examine-worded-amount-one-reagent = вещество.
shared-solution-container-component-on-examine-worded-amount-multiple-reagents = смесь веществ.
examinable-solution-has-recognizable-chemicals = В этом растворе вы можете распознать { $recognizedString }.
examinable-solution-recognized-first = [color={ $color }]{ $chemical }[/color]
examinable-solution-recognized-next = , [color={ $color }]{ $chemical }[/color]
examinable-solution-recognized-last = и [color={ $color }]{ $chemical }[/color]

examinable-solution-on-examine-volume = Ёмкость { $fillLevel ->
    [exact] содержит [color=white]{$current}/{$max}u[/color].
   *[other] [bold]{ -solution-vague-fill-level(fillLevel: $fillLevel) }[/bold].
}

examinable-solution-on-examine-volume-no-max = Ёмкость { $fillLevel ->
    [exact] содержит [color=white]{$current}u[/color].
   *[other] [bold]{ -solution-vague-fill-level(fillLevel: $fillLevel) }[/bold].
}

examinable-solution-on-examine-volume-puddle = Она { $fillLevel ->
    [exact] [color=white]{$current}u[/color].
    [full] переполнена!
    [mostlyfull] переполнена!
    [halffull] очень глубокая.
    [halfempty] глубокая.
   *[mostlyempty] полная.
    [empty] образует множество маленьких лужиц.
}

-solution-vague-fill-level =
    { $fillLevel ->
        [full] [color=white]полна[/color]
        [mostlyfull] [color=#DFDFDF]почти полна[/color]
        [halffull] [color=#C8C8C8]наполовину полна[/color]
        [halfempty] [color=#C8C8C8]наполовину пуста[/color]
        [mostlyempty] [color=#A4A4A4]почти пуста[/color]
       *[empty] [color=gray]пуста[/color]
    }
