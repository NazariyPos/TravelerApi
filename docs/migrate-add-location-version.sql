ALTER TABLE locations
    ADD COLUMN IF NOT EXISTS version INTEGER NOT NULL DEFAULT 1;

ALTER TABLE locations
    ADD CONSTRAINT locations_version_positive
    CHECK (version > 0);
