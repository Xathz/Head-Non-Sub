-- Upgrade the commands table to v3, from commands_v2.sql or a table the bot created
-- Makes command a varchar with an index so /stats does not read every row

-- Check first, this must return 0 or the ALTER below fails without changing anything
SELECT COUNT(*) AS `rows_to_fix` FROM `commands` WHERE `command` IS NULL OR CHAR_LENGTH(`command`) > 255;

ALTER TABLE `commands`
    MODIFY `command` VARCHAR(255) NOT NULL,
    ADD INDEX `server_id_command` (`server_id`, `command`);
