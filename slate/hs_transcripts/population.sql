SET sqlformat csv
SET feedback off
SET heading off

select unique banner_id_match, slate_id, hs_ssid
, extract( year from birthdate ) birth_year
, extract( month from birthdate ) birth_month
, extract( day from birthdate ) birth_day
, district, hs_iden_cde, load_date
from SLATE_PERSON_LOAD
inner join ceeb_lea_crosswalk on ( ceeb = hs_iden_cde
                                       and district is not null )
where hs_iden_cde is not null
and birthdate is not null
-- was loaded into temp table within the last 14 days
and load_date >= (sysdate - 14)
-- has a valid ssid
and ( hs_ssid is not null 
      AND length(hs_ssid) = 7 
      AND NOT regexp_like(hs_ssid, '[A-Za-z]') )
-- hs transcript has yet to be loaded
and hs_transcript_load_date is null;

EXIT;

