CREATE TABLE `short_urls` (
    `id` INT(10) UNSIGNED NOT NULL AUTO_INCREMENT,
    `datetime` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `full_url` TEXT NOT NULL COLLATE 'utf8mb4_unicode_ci',
    `short_url` VARCHAR(50) NOT NULL COLLATE 'utf8mb4_unicode_ci',
    PRIMARY KEY (`id`),
    UNIQUE INDEX `short_url` (`short_url`)
)
COLLATE='utf8mb4_unicode_ci'
ENGINE=InnoDB
;
