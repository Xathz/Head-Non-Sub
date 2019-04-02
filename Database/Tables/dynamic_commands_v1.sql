CREATE TABLE `dynamic_commands` (
    `owner_id` BIGINT(20) UNSIGNED NOT NULL,
    `datetime` DATETIME NOT NULL,
    `command` TINYTEXT NOT NULL COLLATE 'utf8mb4_unicode_ci',
    `text` TEXT NOT NULL COLLATE 'utf8mb4_unicode_ci',
    PRIMARY KEY (`owner_id`)
)
COLLATE='utf8mb4_unicode_ci'
ENGINE=InnoDB
;
