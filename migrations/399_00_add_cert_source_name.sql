ALTER TABLE `documents`.`certificatesources` ADD COLUMN `PersonOrientationName` VARCHAR(255) NOT NULL AFTER `SearchInAssortmentPrice`;

update documents.certificatesources
set PersonOrientationName = "Аптека холдинг воронеж"
where id = 1;

update documents.certificatesources
set PersonOrientationName = "СИА"
where id = 2;

update documents.certificatesources
set PersonOrientationName = "Протек"
where id = 4;

update documents.certificatesources
set PersonOrientationName = "Роста"
where id = 6;

update documents.certificatesources
set PersonOrientationName = "Катрен"
where id = 8;

update documents.certificatesources
set PersonOrientationName = "Фарм комплект"
where id = 10;