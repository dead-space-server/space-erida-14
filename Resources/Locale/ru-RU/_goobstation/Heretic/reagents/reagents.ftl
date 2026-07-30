reagent-name-eldritch = жуткая эссенция
reagent-desc-eldritch = Странная жидкость, которая противоречит законам физики. Она заряжает энергией и исцеляет тех, кто способен заглянуть за пределы этой хрупкой реальности, но невероятно вредна для ограниченных людей.
reagent-comp-condition-heretic-or-ghoul = еретик или гуль
reagent-physical-desc-eldritch = жуткий
flavor-complex-eldritch = Аг'хсдж'садже'ш

reagent-effect-condition-guidebook-has-component =
    цель { $invert ->
                 [true] не имеет
                 *[false] имеет
                } {$comp}

reagent-effect-guidebook-deal-stamina-damage =
    { $chance ->
        [1] { $deltasign ->
                [1] Наносит
                *[-1] Восстанавливает
            }
        *[other]
            { $deltasign ->
                [1] наносит
                *[-1] восстанавливает
            }
    } { $amount } { $immediate ->
                    [true] мгновенный
                    *[false] постепенный
                  } урон выносливости