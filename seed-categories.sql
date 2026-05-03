-- Seed Categories into the database
-- Run this script if EF migrations are not working

-- Clear existing categories (optional - only if you want a fresh start)
-- DELETE FROM "Categories";

-- Insert seed data
INSERT INTO "Categories" ("Id", "Name", "Description") VALUES
(1, 'Electronics', 'Electronic devices and gadgets'),
(2, 'Tools', 'Power tools, hand tools, and equipment'),
(3, 'Vehicles', 'Cars, bikes, and transportation'),
(4, 'Sports', 'Sports equipment and gear'),
(5, 'Home & Garden', 'Home improvement and gardening equipment'),
(6, 'Photography', 'Cameras, lenses, and photography equipment'),
(7, 'Music', 'Musical instruments and audio equipment'),
(8, 'Party & Events', 'Party supplies and event equipment'),
(9, 'Camping & Outdoor', 'Camping gear and outdoor equipment'),
(10, 'Other', 'Miscellaneous items')
ON CONFLICT ("Id") DO NOTHING; -- Don't insert if ID already exists

-- Reset the sequence so new categories start at ID 11
SELECT setval(pg_get_serial_sequence('"Categories"', 'Id'), COALESCE(MAX("Id"), 1)) FROM "Categories";
