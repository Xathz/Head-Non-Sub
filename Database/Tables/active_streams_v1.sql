CREATE TABLE `active_streams` (
    `username` VARCHAR(25) NOT NULL COLLATE 'utf8mb4_unicode_ci',
    `started_at` DATETIME NOT NULL,
    PRIMARY KEY (`username`)
)
COLLATE='utf8mb4_unicode_ci'
ENGINE=InnoDB
;
