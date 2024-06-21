use bd2prj
go

insert into point(p, [data]) values (CONVERT(dbo.Point2d, '20,0'), CAST('<Root><name>X</name><color><r>255</r><g>0</g><b>0</b></color></Root>' AS XML));
insert into point(p, [data]) values (CONVERT(dbo.Point2d, '0,20'), CAST('<Root><name>Y</name><color><r>0</r><g>255</g><b>0</b></color></Root>' AS XML));
insert into point(p, [data]) values (CONVERT(dbo.Point2d, '0,0'), CAST('<Root><name>0, 0</name><color><r>0</r><g>0</g><b>255</b></color></Root>' AS XML));
insert into point(p, [data]) values (CONVERT(dbo.Point2d, '-20,35'), CAST('<Root><name>Made by Leon O¿óg</name><color><r>0</r><g>0</g><b>0</b></color></Root>' AS XML));

insert into polygon(p, [data]) values (CONVERT(dbo.Polygon2d, '30,30; 50,30; 55,40; 40,40; 40,55; 30,50;'), CAST('<Root><name>AAA!!</name><color><r>64</r><g>255</g><b>128</b></color></Root>' AS XML))
insert into polygon(p, [data]) values (CONVERT(dbo.Polygon2d, '-20,45;70,45;70,50;-20,50;'), CAST('<Root><name>BBB!!</name><color><r>128</r><g>64</g><b>64</b></color></Root>' AS XML))
insert into polygon(p, [data]) values (CONVERT(dbo.Polygon2d, '2.5,7.5;5,10;7.5,10;10,7.5;10,5;7.5,2.5;5,2.5;2.5,5;'), CAST('<Root><name>Ma³y poligon 1</name><color><r>156</r><g>23</g><b>94</b></color></Root>' AS XML))
