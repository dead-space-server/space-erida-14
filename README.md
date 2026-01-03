<div class="header" align="center">
<img alt="Space Station 14" width="880" height="300" src="https://raw.githubusercontent.com/space-wizards/asset-dump/de329a7898bb716b9d5ba9a0cd07f38e61f1ed05/github-logo.svg">
</div>

Space Station 14 — это ремейк SS13, работающий на [Robust Toolbox](https://github.com/space-wizards/RobustToolbox), собственном движке, написанном на C#.

Это форк репозитория по Space Station 14. Чтобы предотвратить создание форков RobustToolbox, клиентом и сервером загружается «контент-пак». Этот контент-пак содержит всё необходимое для игры на конкретном сервере.

Если вы хотите разместить сервер или создавать контент для SS14, то вам нужен именно этот репозиторий. Он содержит как RobustToolbox, так и контент-пак для разработки новых контент-паков.

## Ссылки

<div class="header" align="center">

[Веб-сайт](https://wiki.deadspace14.net/%D0%AD%D1%80%D0%B8%D0%B4%D0%B0:%D0%97%D0%B0%D0%B3%D0%BB%D0%B0%D0%B2%D0%BD%D0%B0%D1%8F_%D1%81%D1%82%D1%80%D0%B0%D0%BD%D0%B8%D1%86%D0%B0) | [Discord](https://discord.com/invite/erida)

</div>

## Документация / Вики

На официальном [сайте документации](https://docs.spacestation14.com/) есть документация по контенту SS14, движку, игровому дизайну и многому другому.
Дополнительно ознакомьтесь с этими ресурсами для получения информации о лицензиях и атрибуции:
- [Robust Generic Attribution](https://docs.spacestation14.com/en/specifications/robust-generic-attribution.html)
- [Robust Station Image](https://docs.spacestation14.com/en/specifications/robust-station-image.html)

У нас также есть множество ресурсов для новых участников проекта.

## Участие в разработке

Мы рады принимать вклад от любого человека. Заходите в Discord, если хотите помочь. У нас есть [список задач (issues)](https://github.com/dead-space-server/space-erida-14/issues), которые необходимо выполнить, и любой может взять их на себя. Не бойтесь просить о помощи!
Просто убедитесь, что ваши изменения и pull request'ы соответствуют [рекомендациям по внесению вклада](https://docs.spacestation14.com/en/general-development/codebase-info/pull-request-guidelines.html).

## Сборка

1. Клонируйте этот репозиторий:
```shell
git clone https://github.com/space-wizards/space-station-14.git
```
2. Перейдите в директорию проекта и запустите `RUN_THIS.py` чтобы инициализировать Саб-Модули и скачать движок игры:
```shell
cd space-station-14
python RUN_THIS.py
```
3. Соберите сервер:

Соберите сервер используя `dotnet build`.

[Подробнее](https://docs.spacestation14.com/en/general-development/setup.html)

## Лицензия

Весь код в этой кодовой базе выпущен под лицензией AGPL-3.0-or-later. Каждый файл содержит заголовки спецификации REUSE или отдельные файлы .license, которые указывают опцию двойного лицензирования. Это двойное лицензирование предоставлено для упрощения процесса для проектов, которые не используют AGPL, позволяя им использовать соответствующие части кода по альтернативной лицензии. Вы можете ознакомиться с полными текстами этих лицензий в каталоге LICENSES/.

Большинство медиа-ресурсов лицензированы под [CC-BY-SA 3.0](https://creativecommons.org/licenses/by-sa/3.0/) если не указано иное. Ресурсы имеют свою лицензию и информацию об авторских правах в файле метаданных. [Пример](https://github.com/space-wizards/space-station-14/blob/master/Resources/Textures/Objects/Tools/crowbar.rsi/meta.json).

> [!NOTE]
> Некоторые ресурсы лицензированы по некоммерческой лицензии [CC-BY-NC-SA 3.0](https://creativecommons.org/licenses/by-nc-sa/3.0/) или аналогичным некоммерческим лицензиям, и их необходимо удалить, если вы хотите использовать этот проект в коммерческих целях.
