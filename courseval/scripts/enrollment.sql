SET sqlformat csv
SET feedback off

SELECT ssbsect_crn,
       ssbsect_subj_code || ' ' || DECODE(ssbsect_crn,
                   43969, '0110', -- ESOL
                   43724, '0100',  43729, '0100', 43736, '0100', -- ESOL
                   43730, '0160', 43737, '0160', -- ESOL
                   40667, '2800', -- DANC
                   40432, '1730', -- PEHR
                   ssbsect_crse_numb) AS "Course Number",
                   dsc.get_xlst_master_seq(ssbsect_term_code, ssrxlst_xlst_group, ssbsect_subj_code,
                             ssbsect_crse_numb, ssbsect_seq_numb) AS "Section Number",
         ssbsect_term_code AS "Period",
         spriden_id AS "ID Field"
FROM ssbsect, sfrstcr, spriden, ssrxlst
WHERE ((ssbsect_term_code = sfrstcr_term_code)
AND (ssbsect_crn = sfrstcr_crn))
AND sfrstcr_term_code = f_get_term(sysdate, 'pterm')
AND sfrstcr_rsts_code LIKE 'R%'
AND spriden_pidm = sfrstcr_pidm
AND spriden_change_ind IS NULL
AND ssbsect_crn = ssrxlst_crn(+)
AND ssbsect_term_code = ssrxlst_term_code(+)
AND ssbsect_subj_code NOT IN ('CED', 'ICL')
ORDER BY "Course Number", "Section Number", spriden_id;

EXIT
