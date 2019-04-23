CREATE TABLE `cooldowns` (
    `datetime_offset` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `server_id` BIGINT(20) UNSIGNED NOT NULL,
    `user_id` BIGINT(20) UNSIGNED NOT NULL,
    `command` VARCHAR(300) NOT NULL COLLATE 'utf8mb4_unicode_ci',
    PRIMARY KEY (`server_id`, `user_id`, `command`),
    INDEX `server_id` (`server_id`),
    INDEX `user_id` (`user_id`)
)
COLLATE='utf8mb4_unicode_ci'
ENGINE=InnoDB
;
