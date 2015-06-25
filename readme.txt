Библиотеки

* для генерации регистрационной карты используется
MICROSOFT® REPORT VIEWER 2012 RUNTIME
https://www.microsoft.com/en-US/Download/details.aspx?id=35747

javascript тесты

для запуска тестов нужны
nodejs - cinst.bat nonejs.install
bower - npm install -g bower
grunt - npm install -g grunt
модули из bower
bower install
модули из npm
npm install

для описания зависимостей в теста используется requirejs
конфигурация хранится в файле test/test.js
тесты пишутся на coffeescript

для того что бы запустить тесты нужно выполнить
grunt

что бы скомпилировать coffeescript
grunt coffee

Для подсветки пользователей отключенных биллингом раньше использовался стиль DisabledByBilling
теперь следует переходить к стилю disabled-by-billing и использовать Styler для посветки строк.
User.css ~43 строка
