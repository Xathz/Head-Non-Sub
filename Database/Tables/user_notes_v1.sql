CREATE TABLE `user_notes` (
    `server_id` BIGINT(20) UNSIGNED NOT NULL,
    `user_id` BIGINT(20) UNSIGNED NOT NULL,
    `notes` LONGTEXT NULL DEFAULT NULL COLLATE 'utf8mb4_bin',
    PRIMARY KEY (`server_id`, `user_id`)
)
COLLATE='utf8mb4_unicode_ci'
ENGINE=InnoDB
;
