# Sinh ngucanh_hethong.drawio (mxGraph XML). Chạy: python gen_ngucanh.py
from xml.sax.saxutils import escape
cells=[]; n=[1]
def nid(): n[0]+=1; return f"c{n[0]}"
FONT="fontFamily=Times New Roman;fontSize=13;"
def box(x,y,w,h,text,fill,stroke,extra="",bold=False):
    i=nid()
    st=f"rounded=1;whiteSpace=wrap;html=1;arcSize=8;fillColor={fill};strokeColor={stroke};{FONT}{'fontStyle=1;' if bold else ''}{extra}"
    cells.append(f'<mxCell id="{i}" value="{escape(text)}" style="{st}" vertex="1" parent="1"><mxGeometry x="{x}" y="{y}" width="{w}" height="{h}" as="geometry"/></mxCell>')
    return i
def edge(s,t,label="",style="",pts=None):
    i=nid()
    st=f"edgeStyle=orthogonalEdgeStyle;rounded=0;html=1;endArrow=block;startArrow=block;startFill=1;endFill=1;strokeColor=#333333;{FONT}fontSize=11;labelBackgroundColor=#FFFFFF;{style}"
    g='<mxGeometry relative="1" as="geometry">'+('<Array as="points">'+''.join(f'<mxPoint x="{a}" y="{b}"/>' for a,b in pts)+'</Array>' if pts else '')+'</mxGeometry>'
    cells.append(f'<mxCell id="{i}" value="{escape(label)}" style="{st}" edge="1" parent="1" source="{s}" target="{t}">{g}</mxCell>')
WEB=("#FFF2CC","#D6B656"); ADM=("#DAE8FC","#6C8EBF"); EXT=("#F5F5F5","#666666")
# khung hệ thống
sysid=nid()
cells.append(f'<mxCell id="{sysid}" value="HỆ THỐNG QUẢN LÝ VÀ BÁN SÁCH TRỰC TUYẾN" style="rounded=1;arcSize=3;whiteSpace=wrap;html=1;fillColor=#FAFAFA;strokeColor=#333333;strokeWidth=2;verticalAlign=bottom;spacingBottom=6;fontStyle=1;{FONT}" vertex="1" parent="1"><mxGeometry x="210" y="150" width="500" height="450" as="geometry"/></mxCell>')
web=box(230,215,180,160,"Trang Web bán sách<br>(Blazor)",*WEB,bold=True)
adm=box(510,215,180,160,"Ứng dụng quản trị Desktop<br>(WinUI 3)",*ADM,bold=True)
api=box(510,415,180,55,"Web API",*ADM,bold=True)
db=nid()
cells.append(f'<mxCell id="{db}" value="Cơ sở dữ liệu&lt;br&gt;SQL Server dùng chung" style="shape=cylinder3;whiteSpace=wrap;html=1;boundedLbl=1;backgroundOutline=1;size=12;fillColor=#E1D5E7;strokeColor=#9673A6;fontStyle=1;{FONT}" vertex="1" parent="1"><mxGeometry x="350" y="490" width="220" height="85" as="geometry"/></mxCell>')
# tác nhân
kvl=box(0,225,120,60,"Khách vãng lai",*WEB,bold=True)
kh=box(0,305,120,60,"Khách hàng",*WEB,bold=True)
ql=box(800,265,120,60,"Quản lý",*ADM,bold=True)
vnp=box(230,30,180,60,"Cổng thanh toán VNPay",*EXT,bold=True)
mail=box(510,30,180,60,"Dịch vụ Email",*EXT,bold=True)
ncc=box(800,440,120,60,"Nhà cung cấp",*EXT,"dashed=1;")
dvc=box(800,540,120,60,"Đơn vị vận chuyển",*EXT,"dashed=1;")
# cạnh
edge(kvl,web,"Xem, tìm sách,<br>đăng ký","exitX=1;exitY=0.5;entryX=0;entryY=0.25;")
edge(kh,web,"Giỏ hàng, đặt hàng,<br>theo dõi đơn","exitX=1;exitY=0.5;entryX=0;entryY=0.75;")
edge(ql,adm,"Xử lý đơn, kho,<br>sách, báo cáo","exitX=0;exitY=0.5;entryX=1;entryY=0.5;")
edge(web,vnp,"Yêu cầu / kết quả<br>thanh toán","exitX=0.5;exitY=0;entryX=0.5;entryY=1;")
edge(sysid,mail,"Email xác nhận đơn,<br>đặt lại mật khẩu","startArrow=none;exitX=0.78;exitY=0;entryX=0.5;entryY=1;")
edge(adm,api,"Gọi API","exitX=0.5;exitY=1;entryX=0.5;entryY=0;")
edge(api,db,"","exitX=0.5;exitY=1;entryX=1;entryY=0.5;entryDx=0;entryDy=0;")
edge(web,db,"Tầng nghiệp vụ<br>dùng chung","exitX=0.5;exitY=1;entryX=0;entryY=0.5;entryDx=0;entryDy=0;")
edge(ncc,ql,"Giao sách<br>(phiếu nhập)","dashed=1;exitX=0.5;exitY=0;entryX=0.5;entryY=1;")
edge(dvc,ql,"","dashed=1;exitX=1;exitY=0.5;entryX=1;entryY=0.5;",pts=[(950,570),(950,295)])
# chú thích
cells.append(f'<mxCell id="{nid()}" value="Nét liền: tương tác trực tiếp với hệ thống.&lt;br&gt;Nét đứt: làm việc với nhà sách, thông tin phát sinh do Quản lý cập nhật." style="text;html=1;align=left;verticalAlign=top;whiteSpace=wrap;{FONT}fontSize=11;fontStyle=2;" vertex="1" parent="1"><mxGeometry x="0" y="520" width="190" height="70" as="geometry"/></mxCell>')
xml=('<mxfile host="app.diagrams.net"><diagram name="Ngu canh he thong" id="ngucanh"><mxGraphModel dx="1000" dy="650" grid="0" guides="1" tooltips="1" connect="1" arrows="1" fold="1" page="1" pageScale="1" pageWidth="1000" pageHeight="700" math="0" shadow="0"><root><mxCell id="0"/><mxCell id="1" parent="0"/>'
     +''.join(cells)+'</root></mxGraphModel></diagram></mxfile>')
open("ngucanh_hethong.drawio","w",encoding="utf8").write(xml)
