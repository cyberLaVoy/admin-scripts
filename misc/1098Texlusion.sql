/*
Exlusion Script - so zeros don't print

Here is the script to exclude people from the 1098T processing, with the following modifications:

1) Identify international students using the sgbstdn_rate_code of ISF
2) Removed code that compares box 4 to box 2
3) Prompt for the tax year when it runs, so the script is year independent.
*/

-- Change status to Exclude when student no amounts in any fields
UPDATE ttbtaxn
    SET ttbtaxn.ttbtaxn_stud_notif_status = 'E',
        ttbtaxn.ttbtaxn_irs_report_status = 'E',
        ttbtaxn.ttbtaxn_remove_notif_ind = 'Y'
WHERE NVL (ttbtaxn.ttbtaxn_amount_1, 0) = 0
AND NVL (ttbtaxn.ttbtaxn_amount_2, 0) = 0
AND NVL (ttbtaxn.ttbtaxn_amount_3, 0) = 0
AND NVL (ttbtaxn.ttbtaxn_amount_4, 0) = 0
AND NVL (ttbtaxn.ttbtaxn_amount_5, 0) = 0
AND NVL (ttbtaxn.ttbtaxn_amount_6, 0) = 0
AND ttbtaxn.ttbtaxn_tax_year = &&TaxYr;


-- Change status to Exclude for International students
UPDATE ttbtaxn
SET ttbtaxn.ttbtaxn_stud_notif_status = 'E',
    ttbtaxn.ttbtaxn_irs_report_status = 'E',
    ttbtaxn.ttbtaxn_remove_notif_ind = 'Y'
WHERE ttbtaxn.ttbtaxn_tax_year = &&TaxYr
AND ttbtaxn.ttbtaxn_remove_notif_ind <> 'Y'
AND ttbtaxn.ttbtaxn_pidm IN ( SELECT a.sgbstdn_pidm
                              FROM sgbstdn a
                              WHERE a.sgbstdn_rate_code = 'ISF'
                              AND a.sgbstdn_term_code_eff = ( SELECT MAX (b.sgbstdn_term_code_eff)
                                                              FROM sgbstdn b
                                                              WHERE b.sgbstdn_pidm = a.sgbstdn_pidm
                                                              AND b.sgbstdn_term_code_eff <= &&TaxYr+1||'20'));

-- Change status to Exclude when student has no credit hours for year (ce students)
UPDATE ttbtaxn
SET ttbtaxn.ttbtaxn_stud_notif_status = 'E',
    ttbtaxn.ttbtaxn_irs_report_status = 'E',
    ttbtaxn.ttbtaxn_remove_notif_ind = 'Y'
WHERE ttbtaxn.ttbtaxn_tax_year = &&TaxYr
AND ttbtaxn.ttbtaxn_remove_notif_ind <> 'Y'
AND ttbtaxn.ttbtaxn_pidm IN ( SELECT DISTINCT sfrstcr_pidm
                              FROM sfrstcr, stvrsts
                              WHERE sfrstcr_term_code BETWEEN &&TaxYr||'30' AND &&TaxYr+1||'30'
                              AND stvrsts_incl_sect_enrl = 'Y'
                              AND sfrstcr_rsts_code = stvrsts_code
                              HAVING SUM (NVL (sfrstcr_credit_hr, 0)) = 0
                              GROUP BY sfrstcr_pidm);

-- Change status to Exclude when SSN is missing
UPDATE ttbtaxn
SET ttbtaxn.ttbtaxn_stud_notif_status = 'E',
    ttbtaxn.ttbtaxn_irs_report_status = 'E',
    ttbtaxn.ttbtaxn_remove_notif_ind = 'Y'
WHERE ttbtaxn.ttbtaxn_ssn is null
AND ttbtaxn.ttbtaxn_tax_year = &&TaxYr;


COMMIT;