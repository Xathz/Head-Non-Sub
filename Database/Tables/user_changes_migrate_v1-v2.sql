-- Remove old columns and add new columns
-- 'user_changes_v2.sql' does not need to be applied after this
ALTER TABLE `user_changes`
    DROP COLUMN `old_user_avatar`,
    DROP COLUMN `new_user_avatar`,
    ADD COLUMN `backblaze_avatar_bucket` VARCHAR(50) NULL DEFAULT NULL COLLATE 'utf8mb4_unicode_ci' AFTER `new_user_display`,
    ADD COLUMN `backblaze_avatar_filename` TEXT NULL DEFAULT NULL COLLATE 'utf8mb4_unicode_ci' AFTER `backblaze_avatar_bucket`,
    ADD COLUMN `backblaze_avatar_url` TEXT NULL DEFAULT NULL COLLATE 'utf8mb4_unicode_ci' AFTER `backblaze_avatar_filename`;

-- Delete rows that are only an avatar change
-- This will lose data but the accompanying files are gone
DELETE FROM `user_changes` WHERE `change_type` = 8;

-- Change rows that contain an avatar change, bitwise value
-- https://github.com/Xathz/Head-Non-Sub/blob/a75afce07f38aa6ec5d2f3d4af3aca79f2606024/Head%20Non-Sub/Statistics/StatisticsManager.cs#L22
UPDATE `user_changes` SET `change_type` = `change_type` & ~8 WHERE `change_type` & 8 !=0;
