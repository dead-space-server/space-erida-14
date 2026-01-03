github-command-test-name = testgithubapi

cmd-testgithubapi-desc = Эта команда создает репорт об ошибке через github api. Не забывайте проверять серверную консоль на наличие ошибок.
cmd-testgithubapi-help = Usage: testgithubapi

github-command-not-enabled = API не подключён!
github-command-no-path = Пустой ключ Патча!
github-command-no-app-id = id приложения пустое!
github-command-no-repo-name = Имя репозитория пустое!
github-command-no-owner = Пустое поле Владельца репозитория!

github-command-issue-title-one = Это тестовая ошибка!
github-command-issue-description-one = Это описание первой ошибки. :)

github-command-finish = Проверьте ваш репозиторий на наличие созданной ошибки. Если ничего нету, проверьте консоль сервера на наличие ошибок!

github-issue-format = ## Описание:
                      {$description}

                      ## Мета Data:
                      Версия Билда: {$buildVersion}
                      Версия Движка: {$engineVersion}

                      Название сервера: {$serverName}
                      Submitted time: {$submittedTime}

                      -- Информация о раунде --
                      Номер раунда: {$roundNumber}
                      Время раунда: {$roundTime}
                      Режим раунда: {$roundType}
                      Карта: {$map}
                      Количество игроков: {$numberOfPlayers}

                      -- Информация о упомянувшем --
                      Ник игрока: {$username}
                      GUID Игрока: {$playerGUID}
