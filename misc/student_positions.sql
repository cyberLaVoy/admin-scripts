whenever sqlerror exit sql.sqlcode;
exec DSC.DSU_STUDENTPOSITIONS('&1');
commit;
exit;
