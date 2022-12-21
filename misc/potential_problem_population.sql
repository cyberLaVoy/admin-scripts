select unique f_get_spriden_id(saradap_pidm) banner_id, saradap_term_code_entry application_entry_term
from saradap
left join sorhsch on sorhsch_pidm = saradap_pidm
left join sorlcur on sorlcur.rowid = dsc.f_get_sorlcur_rowid(saradap_pidm,saradap_term_code_entry,'LEARNER')
-- has applied for upcoming semesters
where saradap_term_code_entry in (202120, 202130, 202140)
-- was a previous student
and saradap_pidm in ( select sfrstcr_pidm
                      from sfrstcr
                      where sfrstcr_term_code not in (202120, 202130, 202140) )
-- transcript received before application or received date has not been set
and ( sorhsch_trans_recv_date < saradap_appl_date or sorhsch_trans_recv_date is null )
and sorlcur_program != 'ND-CE'
;
