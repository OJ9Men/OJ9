CREATE SCHEMA `lobby`;

CREATE TABLE `lobby`.`user`
(
    `guid`     VARCHAR(36) NOT NULL,
    `nickname` VARCHAR(30) NOT NULL,
    `rating`   INT NULL,
    PRIMARY KEY (`guid`),
    UNIQUE INDEX `guid_UNIQUE` (`guid` ASC) VISIBLE,
    UNIQUE INDEX `nickname_UNIQUE` (`nickname` ASC) VISIBLE
);
