CREATE TABLE `stalkers` (
    `server_id` BIGINT(20) UNSIGNED NOT NULL,
    `user_id` BIGINT(20) UNSIGNED NOT NULL,
    `stalking_user_id` BIGINT(20) UNSIGNED NOT NULL,
    `datetime` DATETIME NOT NULL,
    PRIMARY KEY (`server_id`, `user_id`, `stalking_user_id`)
)
COLLATE='utf8mb4_unicode_ci'
ENGINE=InnoDB
;
