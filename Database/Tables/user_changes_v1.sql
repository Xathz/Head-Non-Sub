CREATE TABLE `user_changes` (
    `id` INT(10) UNSIGNED NOT NULL AUTO_INCREMENT,
    `datetime` DATETIME NOT NULL,
    `server_id` BIGINT(20) UNSIGNED NULL DEFAULT NULL,
    `user_id` BIGINT(20) UNSIGNED NOT NULL,
    `change_type` SMALLINT(2) UNSIGNED NOT NULL,
    `old_user_name` TEXT NULL DEFAULT NULL COLLATE 'utf8mb4_unicode_ci',
    `new_user_name` TEXT NULL DEFAULT NULL COLLATE 'utf8mb4_unicode_ci',
    `old_user_name_discriminator` VARCHAR(4) NULL DEFAULT NULL COLLATE 'utf8mb4_unicode_ci',
    `new_user_name_discriminator` VARCHAR(4) NULL DEFAULT NULL COLLATE 'utf8mb4_unicode_ci',
    `old_user_display` TEXT NULL DEFAULT NULL COLLATE 'utf8mb4_unicode_ci',
    `new_user_display` TEXT NULL DEFAULT NULL COLLATE 'utf8mb4_unicode_ci',
    `old_user_avatar` TEXT NULL DEFAULT NULL COLLATE 'utf8mb4_unicode_ci',
    `new_user_avatar` TEXT NULL DEFAULT NULL COLLATE 'utf8mb4_unicode_ci',
    PRIMARY KEY (`id`),
    INDEX `user_id` (`user_id`)
)
COLLATE='utf8mb4_unicode_ci'
ENGINE=InnoDB
;
