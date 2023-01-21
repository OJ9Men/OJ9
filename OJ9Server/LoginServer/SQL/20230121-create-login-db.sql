create database login;

create table login.user_temp
(
    id       bigint      not null
        primary key,
    idString varchar(45) not null,
    pw       varchar(45) not null,
    constraint idString_UNIQUE
        unique (idString)
)
    comment 'before using kakao login, temporary applied';

