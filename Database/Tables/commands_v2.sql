CREATE TABLE `commands` (
    `id` INT(10) UNSIGNED NOT NULL AUTO_INCREMENT,
    `datetime` DATETIME NOT NULL,
    `server_id` BIGINT(20) UNSIGNED NOT NULL,
    `channel_id` BIGINT(20) UNSIGNED NOT NULL,
    `user_id` BIGINT(20) UNSIGNED NOT NULL,
    `user_name` TEXT NOT NULL COLLATE 'utf8mb4_unicode_ci',
    `user_display` TEXT NULL DEFAULT NULL COLLATE 'utf8mb4_unicode_ci',
    `message_id` BIGINT(20) UNSIGNED NOT NULL,
    `message` MEDIUMTEXT NULL DEFAULT NULL COLLATE 'utf8mb4_unicode_ci',
    `command` TEXT NOT NULL COLLATE 'utf8mb4_unicode_ci',
    `parameters` TEXT NULL DEFAULT NULL COLLATE 'utf8mb4_unicode_ci',
    `reply_message_id` BIGINT(20) UNSIGNED NULL DEFAULT NULL,
    PRIMARY KEY (`id`)
)
COLLATE='utf8mb4_unicode_ci'
ENGINE=InnoDB
;
