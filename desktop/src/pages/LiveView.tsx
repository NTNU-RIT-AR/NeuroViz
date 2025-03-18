import { ContentBox } from "../components/ContentBox";
import SliderCollection from "../components/SliderCollection";

export default function LiveViewPage() {
  return (
    <>
      <h1>Live View</h1>

      <ContentBox>
        <SliderCollection />
      </ContentBox>
    </>
  );
}
