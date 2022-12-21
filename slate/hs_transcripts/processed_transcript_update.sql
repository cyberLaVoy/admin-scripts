UPDATE dsc.slate_person_load
SET hs_transcript_load_date = sysdate
WHERE banner_id_match = '&1' 
AND TRUNC(load_date) = '&2';

COMMIT;
EXIT;