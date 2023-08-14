drop table info; # for preventing the worst situation

create table info
(
    guid           varchar(64) not null
        primary key
        unique,
    id             varchar(64) not null
        unique,
    pw             varchar(16) not null,
    nick_name      varchar(64) not null
        unique,
    last_login_utc int         null,
    soccer_rate    int         null
);