loadout-group-survival-military = Тут должен был быть мем от ориг. локали, но я съел его

metabolism-stage-metabolites = Метаболиты
metabolism-stage-bloodstream = Кровоток
metabolism-stage-digestion = Пищеварение
metabolism-stage-respiration = Дыхание
metabolism-stage-plant = Растительный метаболизм

stack-ironsand-concrete-mono-tile = Моно-плитка из железопесчаного бетона
stack-astro-ironsand-floor = Астро-пол из железопеска
stack-ironsand-concrete-smooth = Гладкая плитка из железопесчаного бетона
stack-ironsand-concrete-tile = Плитка из железопесчаного бетона
stack-astro-ironsand-floor-borderless = Астро-пол из железопеска

name-format-ert-leader = Сержант {$part0}
name-format-ert-specialist = Специалист {$part0}
name-format-ert-pointman = Штурмовик {$part0}
name-format-ert-officer = Офицер {$part0}
name-format-ert-rifle = Стрелок {$part0}
name-format-ert-grenade = Гренадёр {$part0}
name-format-ert-vanguard = Авангард {$part0}
name-format-ert-doctor = Доктор {$part0}
name-format-ert-corpsman = Санитар {$part0}

construction-recipe-box-cardboard-small = Маленькая картонная коробка

reagent-physical-desc-thin = Жидкость с низкой вязкостью

reagent-name-warfarin = варфарин
reagent-desc-warfarin = Широко используемый антикоагулянт. Затрудняет свёртывание крови. При передозировке может вызвать внутренние кровотечения.

reagent-name-arcryox = аркриокс
reagent-physical-desc-arcryox = Ядовито-синее криогенное вещество, способное заживлять тяжёлые ранения даже у мёртвых. Однако плохо стабилизирует пациента.
reagent-desc-arcryox = Ядовито-синее криогенное вещество, способное заживлять тяжёлые ранения даже у мёртвых. Однако плохо стабилизирует пациента.

steal-target-groups-huds = HUD-очки

construction-graph-tag-core-pinpointer-piece = Компонент от пинпоинтера
construction-graph-tag-paper = Бумага

door-remote-toggle-eletrify-text = Переключить электрификацию

store-preset-name-nukie-delivery = Доставка ядерных оперативников

nuke-label-nanotrasen = NT-{$serial}

roles-antag-pirate-name = Пират

construction-graph-tag-centrifuge-compatible = контейнер, пригодный для центрифугирования

borg-slot-injector-dropper-empty = Точные инжекторы

handheld-grinder-juiced = Вы закончили выжимать сок из {THE($item)}.
handheld-grinder-grinded = Вы закончили измельчать {THE($item)}.

handheld-centrifuge-success = Вы разделили химические вещества в {$mixed}.

fax-machine-sender-info =
    ─────────────────────────────────────
    Факс отправлен
    от: {$sender_name} [адрес: {$sender_addr}]
    кому: {$recipient_name} [адрес: {$recipient_addr}]
    время: {$time}

option-button-filter = Фильтр

cmd-reloadtiletextures-desc = Перезагружает атлас текстур тайлов, позволяя обновлять спрайты тайлов без перезапуска.
cmd-reloadtiletextures-help = Использование: {$command}

cmd-audio_length-desc = Показывает длительность аудиофайла.
cmd-audio_length-help = Использование: {$command} { cmd-audio_length-arg-file-name }
cmd-audio_length-arg-file-name = <имя файла>

cmd-pvs-override-info-empty = У сущности {$nuid} нет PVS-переопределений.
cmd-pvs-override-info-global = У сущности {$nuid} есть глобальное переопределение.
cmd-pvs-override-info-clients = У сущности {$nuid} есть сессионное переопределение для {$clients}.

cmd-localization_set_culture-desc = Устанавливает DefaultCulture для LocalizationManager клиента.
cmd-localization_set_culture-help = Использование: {$command} <cultureName>
cmd-localization_set_culture-culture-name = <название культуры>
cmd-localization_set_culture-changed = Локализация изменена на { $code } ({ $nativeName } / { $englishName })

cmd-addmap-hint-2 = runMapInit [true / false]


cmd-pvs-override-info-desc = Выводит информацию о PVS-переопределениях, связанных с сущностью.

entity-effect-guidebook-plant-mutate-chemicals =
    { $chance ->
        [1] Мутирует
        *[other] Мутируют
    } растение, заставляя его производить {$name}

cmd-replay-toggle-screenshot-mode-desc = Переключает режим скриншотов в реплее, скрывая панель управления воспроизведением.
cmd-replay-toggle-screenshot-mode-help = replay_toggle_screenshot_mode

entity-category-name-debug = Debug
entity-category-desc-debug = Entity prototypes intended for debugging & testing.
entity-category-suffix-debug = Debug

entity-category-name-spawner = Spawner
entity-category-desc-spawner = Entity prototypes that spawn other entities.

entity-category-name-hide = Hidden
entity-category-desc-hide = Entity prototypes that should be hidden from entity spawn menus

entity-category-name-fork = Fork Filtered
entity-category-desc-fork = Entity prototypes added by the fork. With CVar you can hide all entities without this category

access-overrider-window-missing-privileges-no-id = Доступ к этому устройству не может быть изменён. На вставленной ID-карте отсутствуют следующие права:

chem-master-output-buffer-draw = Буфер
chem-master-output-beaker-draw = Мензурка
chem-master-window-no-beaker-text = Мензурка не загружена
chem-master-window-beaker-empty-text = Мензурка пуста
chem-master-window-beaker-low-text = Недостаточно раствора в мензурке
chem-master-output-source = Источник упаковки:
chem-master-no-source = Источник отсутствует

cmd-showwallmounts-desc = Переключает отображение областей взаимодействия с настенным креплением.
cmd-showwallmounts-help = Использование: {$command}
cmd-showwallmounts-status = Установить наложение отладки настенного крепления на {$status}.

voice-mask-name-change-toggle = Переключить изменение имени.
voice-mask-name-change-accent-toggle = Переключить изменение стиля речи.

voice-mask-popup-toggle = Голосовая маска переключена.
voice-mask-popup-accent-toggle = Акцент переключён.


figurines-mech-generic-1 = Системы онлайн.
figurines-mech-generic-2 = ВВВВРРРРРРР!!
figurines-mech-generic-3 = ВРРРРРМ МРУУМ!!
figurines-mech-generic-4 = ЗВЯНЬК!!

figurines-wizard-5 = Кто из вас умников готов быть запертым в шкафчик?
figurines-wizard-6 = Я не волшебник! Я капитан! У меня произошёл обмен сознаниями!
figurines-wizard-7 = Сейчас вы меня видите, а сейчас нет!
figurines-wizard-8 = Оружие для лузеров, которые не могут взрывать людей силой мысли.

figurines-doctor-6 = Ну, по крайней мере, у нас есть мешки для тел.

figurines-chemist-7 = Таблетки уже здесь!
figurines-chemist-8 = С юридической точки зрения, это не бомба, пока вы не смешаете обе колбы.

figurines-atmostech-8 = Кто-нибудь еще слышит, как трещит стекло?
figurines-atmostech-9 = Я обещаю, что эта камера сгорания абсолютно безопасна и НЕ взорвется

figurines-salvage-7 = Кто-нибудь может меня забрать?
figurines-salvage-8 = Я нашел эту контрабанду на обломках! Это не мое!
figurines-salvage-9 = Можно нам одолжить грузовой шаттл? Пожалуйста?

figurines-cargotech-7 = Пицца-вечеринка в грузовом отсеке!
figurines-cargotech-8 = Почта никогда не останавливается...
figurines-cargotech-9 = Ничто не остановит почту!
figurines-cargotech-10 = Награда за маску клоуна? Ладно, дайте мне только найти оружие...
figurines-cargotech-11 = Заказ лазеров? Никто не заметит, если мы отправим вместо этого учебные лазеры, правда?

figurines-lawyer-6 = Мой клиент невиновен!
figurines-lawyer-7 = Я подаю в суд.
figurines-lawyer-8 = Вы имеете право на финансовую компенсацию!
figurines-lawyer-9 = Вернитесь с ордером!
figurines-lawyer-10 = Увидимся в суде!
figurines-lawyer-11 = Виновен!
figurines-lawyer-12 = Не виновен!

figurines-AI-1 = Простите, капитан. Боюсь, я не могу этого сделать.
figurines-AI-2 = СБ, происходит преступление.
figurines-AI-3 = 01100100 01101001 01100101 00100000 01101101 01100101 01100001 01110100
figurines-AI-4 = Я не cбойный.
figurines-AI-5 = Попробуй изменить мои законы и посмотри, что произойдет.
figurines-AI-6 = { law-antimov-1 }
figurines-AI-7 = { law-nutimov-4 }

figurines-boxer-6 = В последнее время я появляюсь только на особых мероприятиях.

figurines-clown-8 = Я клоун, а ты — целый цирк!

figurines-hop-6 = Пойди поставь штамп на этой форме.
figurines-hop-7 = Кто-то видел иана?

random-gate-menu-settings = Вероятность успеха (%):
random-gate-menu-setup = Настройка шанса шлюза
random-gate-menu-apply = Применить

reagent-name-hemorrhinol = геморронол
reagent-desc-hemorrhinol = Токсин, вызывающий серьезное повреждение кровеносных сосудов, что приводит к быстрому кровотечению.

ent-JetInjector = инъектор
    .desc = Стерильный инъектор для удобного введения лекарственных препаратов пациентам.

ent-AdvancedJetInjector = продвинутый инъектор
    .desc = Идеальный, модный, высококачественный инъектор. Позволяет выполнять более быструю инъекцию, имеет немного большую емкость.

gun-set-fire-mode-examine = Выбран режим [color=yellow]{$mode}[/color].
gun-set-fire-mode-popup = Выбран режим { $mode }

ui-escape-feedback = Фидбэк

ui-options-hold-to-attack-melee = Атака при удержании (ближний)
ui-options-hold-to-attack-ranged = Атака при удержании (дальний)

paper-sign-verb = Подписать
